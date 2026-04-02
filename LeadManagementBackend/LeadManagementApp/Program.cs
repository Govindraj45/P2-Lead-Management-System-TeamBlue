using System.Text;
using Consul;
using LeadManagementSystem.Auth;
using LeadManagementSystem.Consul;
using LeadManagementSystem.Data;
using LeadManagementSystem.Features.Common;
using LeadManagementSystem.Features.Interactions;
using LeadManagementSystem.Features.Leads;
using LeadManagementSystem.Features.Reports;
using LeadManagementSystem.Features.SalesReps;
using LeadManagementSystem.Interfaces;
using LeadManagementSystem.Logic;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// SQL Server via EF Core
builder.Services.AddDbContext<LeadDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.Configure<ConsulSettings>(
    builder.Configuration.GetSection(ConsulSettings.SectionName));

builder.Services.AddSingleton<IConsulClient>(_ =>
{
    var consulAddress = builder.Configuration[$"{ConsulSettings.SectionName}:Address"] ?? "http://localhost:8500";
    return new ConsulClient(config => { config.Address = new Uri(consulAddress); });
});
builder.Services.AddHostedService<ConsulRegistrationHostedService>();

builder.Services.AddMediatR(typeof(Program).Assembly);

builder.Services.AddScoped<ILeadRepository, EfLeadRepository>();
builder.Services.AddScoped<ISalesRepository, EfSalesRepository>();
builder.Services.AddScoped<IInteractionRepository, EfInteractionRepository>();
builder.Services.AddScoped<LeadService>();
builder.Services.AddScoped<ReportService>();

// JWT Authentication
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));
builder.Services.AddSingleton<TokenService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<SeedDataService>();

var jwtSecret = builder.Configuration[$"{JwtSettings.SectionName}:Secret"]!;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration[$"{JwtSettings.SectionName}:Issuer"],
            ValidAudience = builder.Configuration[$"{JwtSettings.SectionName}:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
        };
    });
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("ManagerOrAdmin", policy => policy.RequireRole("SalesManager", "Admin"));
    options.AddPolicy("AllRoles", policy => policy.RequireRole("SalesRep", "SalesManager", "Admin"));
});

// Redis Distributed Cache
var redisConnection = builder.Configuration["Redis:ConnectionString"] ?? "localhost:6379";
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnection;
    options.InstanceName = "LMS_";
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

var app = builder.Build();

// Apply migrations and seed test users
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<LeadDbContext>();
    db.Database.Migrate();

    var seeder = scope.ServiceProvider.GetRequiredService<SeedDataService>();
    await seeder.SeedUsersAsync();
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Results.Ok(new
{
    service = "LeadManagementApp API",
    status = "running",
    database = "SQL Server",
    discovery = "Consul",
    architecture = "Mediator + EF Core"
}));

app.MapGet("/api/health", () => Results.Ok(new
{
    service = "LeadManagementApp",
    status = "Healthy",
    utcTime = DateTime.UtcNow
}));

// Auth endpoint
app.MapPost("/api/auth/login", async (LoginRequest request, AuthService authService) =>
{
    if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
    {
        return Results.BadRequest(new { message = "Email and password are required." });
    }

    var token = await authService.LoginAsync(request.Email, request.Password);
    if (token is null)
    {
        return Results.Unauthorized();
    }

    return Results.Ok(new { token });
});

var leads = app.MapGroup("/api/leads").RequireAuthorization("AllRoles");

leads.MapGet("", async (int? page, int? pageSize, IMediator mediator) =>
{
    var allLeads = await mediator.Send(new GetAllLeadsQuery());
    var p = page ?? 1;
    var ps = pageSize ?? 10;
    var total = allLeads.Count;
    var items = allLeads.Skip((p - 1) * ps).Take(ps).ToList();
    return Results.Ok(new { items, total, page = p, pageSize = ps, totalPages = (int)Math.Ceiling(total / (double)ps) });
});

leads.MapGet("/{id:int}", async (int id, IMediator mediator) =>
{
    var lead = await mediator.Send(new GetLeadByIdQuery(id));
    return lead is null ? Results.NotFound(new { message = "Lead not found." }) : Results.Ok(lead);
});

leads.MapPost("", async (CreateLeadRequest request, IMediator mediator, IDistributedCache cache) =>
{
    var result = await mediator.Send(new CreateLeadCommand(
        request.Name,
        request.Email,
        request.Phone,
        request.Company,
        request.Position,
        request.Status,
        request.Source,
        request.Priority,
        request.AssignedToRepId));

    if (result.Success) await InvalidateAnalyticsCache(cache);
    return result.Success
        ? Results.Created($"/api/leads/{result.Value}", new { id = result.Value, message = result.Message })
        : Results.BadRequest(new { message = result.Message });
});

leads.MapPut("/{id:int}", async (int id, UpdateLeadRequest request, IMediator mediator, IDistributedCache cache) =>
{
    var result = await mediator.Send(new UpdateLeadCommand(
        id,
        request.Name,
        request.Email,
        request.Phone,
        request.Company,
        request.Position,
        request.Status,
        request.Source,
        request.Priority,
        request.AssignedToRepId));

    if (result.Success) await InvalidateAnalyticsCache(cache);
    return ToHttpResult(result, missingResourceStatusCode: StatusCodes.Status404NotFound);
});

leads.MapDelete("/{id:int}", async (int id, IMediator mediator, IDistributedCache cache) =>
{
    var result = await mediator.Send(new DeleteLeadCommand(id));
    if (result.Success) await InvalidateAnalyticsCache(cache);
    return ToHttpResult(result, missingResourceStatusCode: StatusCodes.Status404NotFound);
}).RequireAuthorization("AdminOnly");

leads.MapPut("/{id:int}/status", async (int id, LeadStatusUpdateRequest request, IMediator mediator, IDistributedCache cache) =>
{
    var result = await mediator.Send(new UpdateLeadStatusCommand(id, request.NewStatus));
    if (result.Success) await InvalidateAnalyticsCache(cache);
    return ToHttpResult(result, missingResourceStatusCode: StatusCodes.Status404NotFound);
});

leads.MapPost("/{id:int}/convert", async (int id, IMediator mediator, IDistributedCache cache) =>
{
    var result = await mediator.Send(new ConvertLeadToCustomerCommand(id));
    if (result.Success) await InvalidateAnalyticsCache(cache);
    return ToHttpResult(result, missingResourceStatusCode: StatusCodes.Status404NotFound);
}).RequireAuthorization("ManagerOrAdmin");

// Interaction routes nested under leads (per PDF spec)
leads.MapGet("/{id:int}/interactions", async (int id, IMediator mediator) =>
{
    var items = await mediator.Send(new GetInteractionsByLeadQuery(id));
    return Results.Ok(items);
});

leads.MapPost("/{id:int}/interactions", async (int id, CreateInteractionRequest request, IMediator mediator, IDistributedCache cache) =>
{
    var result = await mediator.Send(new CreateInteractionCommand(
        request.InteractionType,
        request.Notes,
        request.InteractionDate,
        request.FollowUpDate,
        id));

    if (result.Success) await InvalidateAnalyticsCache(cache);
    return result.Success
        ? Results.Created($"/api/leads/{id}/interactions/{result.Value}", new { id = result.Value, message = result.Message })
        : Results.BadRequest(new { message = result.Message });
});

var reps = app.MapGroup("/api/reps").RequireAuthorization("AllRoles");

reps.MapGet("", async (IMediator mediator) =>
{
    var reps = await mediator.Send(new GetAllSalesRepsQuery());
    return Results.Ok(reps);
});

reps.MapGet("/{id:int}", async (int id, IMediator mediator) =>
{
    var rep = await mediator.Send(new GetSalesRepByIdQuery(id));
    return rep is null ? Results.NotFound(new { message = "Sales representative not found." }) : Results.Ok(rep);
});

reps.MapPost("", async (CreateSalesRepRequest request, IMediator mediator) =>
{
    var result = await mediator.Send(new CreateSalesRepCommand(request.Name, request.Email, request.Department));
    return result.Success
        ? Results.Created($"/api/reps/{result.Value}", new { id = result.Value, message = result.Message })
        : Results.BadRequest(new { message = result.Message });
});

reps.MapPut("/{id:int}", async (int id, UpdateSalesRepRequest request, IMediator mediator) =>
{
    var result = await mediator.Send(new UpdateSalesRepCommand(id, request.Name, request.Email, request.Department));
    return ToHttpResult(result, missingResourceStatusCode: StatusCodes.Status404NotFound);
});

reps.MapDelete("/{id:int}", async (int id, IMediator mediator) =>
{
    var result = await mediator.Send(new DeleteSalesRepCommand(id));
    return ToHttpResult(result, missingResourceStatusCode: StatusCodes.Status404NotFound);
});

// Legacy interaction routes (backward-compat)
var interactions = app.MapGroup("/api/interactions").RequireAuthorization("AllRoles");

interactions.MapGet("/lead/{leadId:int}", async (int leadId, IMediator mediator) =>
{
    var items = await mediator.Send(new GetInteractionsByLeadQuery(leadId));
    return Results.Ok(items);
});

interactions.MapPost("", async (CreateInteractionRequest request, IMediator mediator, IDistributedCache cache) =>
{
    var result = await mediator.Send(new CreateInteractionCommand(
        request.InteractionType,
        request.Notes,
        request.InteractionDate,
        request.FollowUpDate,
        request.LeadId));

    if (result.Success) await InvalidateAnalyticsCache(cache);
    return result.Success
        ? Results.Created($"/api/interactions/{result.Value}", new { id = result.Value, message = result.Message })
        : Results.BadRequest(new { message = result.Message });
});

// Analytics endpoints (Redis cached) — matches PDF spec paths
var analytics = app.MapGroup("/api/leads/analytics").RequireAuthorization("AllRoles");

analytics.MapGet("/by-source", async (ReportService reportService, IDistributedCache cache) =>
{
    return await GetCachedOrCompute(cache, "analytics:by-source", () => reportService.GetLeadsBySource());
});

analytics.MapGet("/conversion-rate", async (ReportService reportService, IDistributedCache cache) =>
{
    return await GetCachedOrCompute(cache, "analytics:conversion-rate", () => reportService.GetConversionRate());
});

analytics.MapGet("/by-status", async (ReportService reportService, IDistributedCache cache) =>
{
    return await GetCachedOrCompute(cache, "analytics:by-status", () => reportService.GetLeadStatusDistribution());
});

analytics.MapGet("/by-salesrep", async (ReportService reportService, IDistributedCache cache) =>
{
    return await GetCachedOrCompute(cache, "analytics:by-salesrep", () => reportService.GetLeadsBySalesRep());
});

// Legacy report routes (backward-compat for frontend)
var reports = app.MapGroup("/api/reports").RequireAuthorization("AllRoles");

reports.MapGet("/status-distribution", async (ReportService reportService, IDistributedCache cache) =>
{
    return await GetCachedOrCompute(cache, "analytics:by-status", () => reportService.GetLeadStatusDistribution());
});

reports.MapGet("/by-source", async (ReportService reportService, IDistributedCache cache) =>
{
    return await GetCachedOrCompute(cache, "analytics:by-source", () => reportService.GetLeadsBySource());
});

reports.MapGet("/conversion-rate", async (ReportService reportService, IDistributedCache cache) =>
{
    return await GetCachedOrCompute(cache, "analytics:conversion-rate", () => reportService.GetConversionRate());
});

reports.MapGet("/by-salesrep", async (ReportService reportService, IDistributedCache cache) =>
{
    return await GetCachedOrCompute(cache, "analytics:by-salesrep", () => reportService.GetLeadsBySalesRep());
});

app.Run();

static IResult ToHttpResult(OperationResult result, int missingResourceStatusCode)
{
    if (result.Success)
    {
        return Results.Ok(new { message = result.Message });
    }

    return result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase)
        ? Results.Json(new { message = result.Message }, statusCode: missingResourceStatusCode)
        : Results.BadRequest(new { message = result.Message });
}

static async Task<IResult> GetCachedOrCompute<T>(IDistributedCache cache, string key, Func<T> compute)
{
    var cached = await cache.GetStringAsync(key);
    if (cached is not null)
        return Results.Ok(JsonSerializer.Deserialize<T>(cached));

    var data = compute();
    var json = JsonSerializer.Serialize(data);
    await cache.SetStringAsync(key, json, new DistributedCacheEntryOptions
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
    });
    return Results.Ok(data);
}

static async Task InvalidateAnalyticsCache(IDistributedCache cache)
{
    var keys = new[] { "analytics:by-source", "analytics:conversion-rate", "analytics:by-status", "analytics:by-salesrep" };
    foreach (var key in keys)
        await cache.RemoveAsync(key);
}

public sealed record CreateLeadRequest(
    string Name,
    string? Email,
    string? Phone,
    string? Company,
    string? Position,
    string? Status,
    string? Source,
    string? Priority,
    int? AssignedToRepId);

public sealed record LoginRequest(string Email, string Password);

public sealed record UpdateLeadRequest(
    string Name,
    string? Email,
    string? Phone,
    string? Company,
    string? Position,
    string Status,
    string Source,
    string Priority,
    int? AssignedToRepId);

public sealed record LeadStatusUpdateRequest(string NewStatus);

public sealed record CreateSalesRepRequest(string Name, string Email, string? Department);

public sealed record UpdateSalesRepRequest(string Name, string Email, string Department);

public sealed record CreateInteractionRequest(
    string InteractionType,
    string Notes,
    DateTime? InteractionDate,
    DateTime? FollowUpDate,
    int LeadId);
