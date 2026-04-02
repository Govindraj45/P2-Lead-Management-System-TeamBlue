// ===== IMPORTS: These bring in the tools/libraries this app needs =====
using System.Text;
using Consul;
using LeadManagementSystem.Auth;
using LeadManagementSystem.Consul;
using LeadManagementSystem.Data;
using LeadManagementSystem.Features.Common;
using LeadManagementSystem.Features.Interactions;
using LeadManagementSystem.Features.Leads;
using LeadManagementSystem.Features.Reports;
using LeadManagementSystem.Interfaces;
using LeadManagementSystem.Logic;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json;

// Create the web application builder — this is the starting point for configuring the app
var builder = WebApplication.CreateBuilder(args);

// ===== DATABASE SETUP: Connect to SQL Server using Entity Framework Core =====
builder.Services.AddDbContext<LeadDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ===== SERVICE DISCOVERY: Register with Consul so other services can find this API =====
builder.Services.Configure<ConsulSettings>(
    builder.Configuration.GetSection(ConsulSettings.SectionName));

builder.Services.AddSingleton<IConsulClient>(_ =>
{
    var consulAddress = builder.Configuration[$"{ConsulSettings.SectionName}:Address"] ?? "http://localhost:8500";
    return new ConsulClient(config => { config.Address = new Uri(consulAddress); });
});
builder.Services.AddHostedService<ConsulRegistrationHostedService>();

// ===== CQRS HANDLERS: Register each command/query handler for dependency injection =====
// Each handler does ONE job (create, read, update, delete, etc.) — this is the CQRS pattern
builder.Services.AddScoped<CreateLeadHandler>();
builder.Services.AddScoped<GetAllLeadsHandler>();
builder.Services.AddScoped<GetLeadByIdHandler>();
builder.Services.AddScoped<UpdateLeadHandler>();
builder.Services.AddScoped<UpdateLeadStatusHandler>();
builder.Services.AddScoped<DeleteLeadHandler>();
builder.Services.AddScoped<ConvertLeadToCustomerHandler>();
builder.Services.AddScoped<CreateInteractionHandler>();
builder.Services.AddScoped<GetInteractionsByLeadHandler>();
builder.Services.AddScoped<GetLeadStatusDistributionHandler>();

// Register repositories (data access) and business logic services
builder.Services.AddScoped<ILeadRepository, EfLeadRepository>();
builder.Services.AddScoped<IInteractionRepository, EfInteractionRepository>();
builder.Services.AddScoped<LeadService>();
builder.Services.AddScoped<ReportService>();

// ===== JWT AUTHENTICATION: Set up token-based security =====
// JWT (JSON Web Token) lets us verify who is making each request
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));
builder.Services.AddSingleton<TokenService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<SeedDataService>();

// Read the secret key from settings and configure how tokens are validated
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

// ===== AUTHORIZATION POLICIES: Define who can access what =====
// "AdminOnly" = only Admins, "ManagerOrAdmin" = Managers + Admins, "AllRoles" = everyone logged in
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("ManagerOrAdmin", policy => policy.RequireRole("SalesManager", "Admin"));
    options.AddPolicy("AllRoles", policy => policy.RequireRole("SalesRep", "SalesManager", "Admin"));
});

// ===== REDIS CACHE: Set up Redis for caching analytics data (speeds up reports) =====
var redisConnection = builder.Configuration["Redis:ConnectionString"] ?? "localhost:6379";
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnection;
    options.InstanceName = "LMS_";
});

// ===== CORS: Allow the frontend (React app) to call this API from a different URL =====
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

// Build the app — all services are now registered
var app = builder.Build();

// ===== STARTUP: Run database migrations and create default test users =====
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<LeadDbContext>();
    // Apply any pending database schema changes automatically
    db.Database.Migrate();

    // Seed (create) default users so we can log in immediately
    var seeder = scope.ServiceProvider.GetRequiredService<SeedDataService>();
    await seeder.SeedUsersAsync();
}

// ===== MIDDLEWARE PIPELINE: These run on every request, in order =====
app.UseCors();           // Allow cross-origin requests
app.UseAuthentication(); // Check who the user is (via JWT token)
app.UseAuthorization();  // Check if the user has permission

// ===== ROOT ENDPOINT: Returns basic info when someone visits the API's base URL =====
app.MapGet("/", () => Results.Ok(new
{
    service = "LeadManagementApp API",
    status = "running",
    database = "SQL Server",
    discovery = "Consul",
    architecture = "CQRS + EF Core"
}));

// ===== HEALTH CHECK: A simple endpoint to verify the API is alive and responding =====
app.MapGet("/api/health", () => Results.Ok(new
{
    service = "LeadManagementApp",
    status = "Healthy",
    utcTime = DateTime.UtcNow
}));

// ===== LOGIN ENDPOINT: Users send email + password, get back a JWT token =====
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

// ===== LEADS ENDPOINTS: All routes under /api/leads require a logged-in user =====
var leads = app.MapGroup("/api/leads").RequireAuthorization("AllRoles");

// GET all leads with optional filtering (by status, source, search text) and pagination
leads.MapGet("", async (int? page, int? pageSize, string? status, string? source, string? search, GetAllLeadsHandler handler) =>
{
    var allLeads = await handler.HandleAsync(new GetAllLeadsQuery());

    if (!string.IsNullOrWhiteSpace(status))
        allLeads = allLeads.Where(l => l.Status.Equals(status, StringComparison.OrdinalIgnoreCase)).ToList();
    if (!string.IsNullOrWhiteSpace(source))
        allLeads = allLeads.Where(l => l.Source.Equals(source, StringComparison.OrdinalIgnoreCase)).ToList();
    if (!string.IsNullOrWhiteSpace(search))
        allLeads = allLeads.Where(l =>
            (l.Name != null && l.Name.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
            (l.Email != null && l.Email.Contains(search, StringComparison.OrdinalIgnoreCase))).ToList();

    var p = page ?? 1;
    var ps = pageSize ?? 10;
    var total = allLeads.Count;
    var items = allLeads.Skip((p - 1) * ps).Take(ps).ToList();
    return Results.Ok(new { items, total, page = p, pageSize = ps, totalPages = (int)Math.Ceiling(total / (double)ps) });
});

// GET a single lead by its ID (returns 404 if not found)
leads.MapGet("/{id:int}", async (int id, GetLeadByIdHandler handler) =>
{
    var lead = await handler.HandleAsync(new GetLeadByIdQuery(id));
    return lead is null ? Results.NotFound(new { message = "Lead not found." }) : Results.Ok(lead);
});

// POST a new lead — creates it in the database and clears the analytics cache
leads.MapPost("", async (CreateLeadRequest request, CreateLeadHandler handler, IDistributedCache cache) =>
{
    var result = await handler.HandleAsync(new CreateLeadCommand(
        request.Name,
        request.Email,
        request.Phone,
        request.Company,
        request.Position,
        request.Status,
        request.Source,
        request.Priority,
        request.AssignedSalesRepId));

    if (result.Success) await InvalidateAnalyticsCache(cache);
    return result.Success
        ? Results.Created($"/api/leads/{result.Value}", new { id = result.Value, message = result.Message })
        : Results.BadRequest(new { message = result.Message });
});

// PUT (update) an existing lead by ID — clears the analytics cache on success
leads.MapPut("/{id:int}", async (int id, UpdateLeadRequest request, UpdateLeadHandler handler, IDistributedCache cache) =>
{
    var result = await handler.HandleAsync(new UpdateLeadCommand(
        id,
        request.Name,
        request.Email,
        request.Phone,
        request.Company,
        request.Position,
        request.Status,
        request.Source,
        request.Priority,
        request.AssignedSalesRepId));

    if (result.Success) await InvalidateAnalyticsCache(cache);
    return ToHttpResult(result, missingResourceStatusCode: StatusCodes.Status404NotFound);
});

// DELETE a lead — only Admins can do this
leads.MapDelete("/{id:int}", async (int id, DeleteLeadHandler handler, IDistributedCache cache) =>
{
    var result = await handler.HandleAsync(new DeleteLeadCommand(id));
    if (result.Success) await InvalidateAnalyticsCache(cache);
    return ToHttpResult(result, missingResourceStatusCode: StatusCodes.Status404NotFound);
}).RequireAuthorization("AdminOnly");

// PUT to change just the status of a lead (e.g., New → Contacted)
leads.MapPut("/{id:int}/status", async (int id, LeadStatusUpdateRequest request, UpdateLeadStatusHandler handler, IDistributedCache cache) =>
{
    var result = await handler.HandleAsync(new UpdateLeadStatusCommand(id, request.NewStatus));
    if (result.Success) await InvalidateAnalyticsCache(cache);
    return ToHttpResult(result, missingResourceStatusCode: StatusCodes.Status404NotFound);
});

// POST to convert a qualified lead into a customer — only Managers or Admins can do this
leads.MapPost("/{id:int}/convert", async (int id, ConvertLeadToCustomerHandler handler, IDistributedCache cache) =>
{
    var result = await handler.HandleAsync(new ConvertLeadToCustomerCommand(id));
    if (result.Success) await InvalidateAnalyticsCache(cache);
    return ToHttpResult(result, missingResourceStatusCode: StatusCodes.Status404NotFound);
}).RequireAuthorization("ManagerOrAdmin");

// ===== INTERACTION ROUTES (nested under leads): View and add interactions for a specific lead =====
// GET all interactions for a specific lead
leads.MapGet("/{id:int}/interactions", async (int id, GetInteractionsByLeadHandler handler) =>
{
    var items = await handler.HandleAsync(new GetInteractionsByLeadQuery(id));
    return Results.Ok(items);
});

// POST a new interaction (e.g., phone call, email) for a lead
leads.MapPost("/{id:int}/interactions", async (int id, CreateInteractionRequest request, CreateInteractionHandler handler, IDistributedCache cache) =>
{
    var result = await handler.HandleAsync(new CreateInteractionCommand(
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

// ===== LEGACY INTERACTION ROUTES: Older URL format kept for backward compatibility =====
var interactions = app.MapGroup("/api/interactions").RequireAuthorization("AllRoles");

interactions.MapGet("/lead/{leadId:int}", async (int leadId, GetInteractionsByLeadHandler handler) =>
{
    var items = await handler.HandleAsync(new GetInteractionsByLeadQuery(leadId));
    return Results.Ok(items);
});

interactions.MapPost("", async (CreateInteractionRequest request, CreateInteractionHandler handler, IDistributedCache cache) =>
{
    var result = await handler.HandleAsync(new CreateInteractionCommand(
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

// ===== ANALYTICS ENDPOINTS: Reports with Redis caching (data cached for 5 minutes) =====
var analytics = app.MapGroup("/api/leads/analytics").RequireAuthorization("AllRoles");

// How many leads came from each source (Website, Referral, etc.)
analytics.MapGet("/by-source", async (ReportService reportService, IDistributedCache cache) =>
{
    return await GetCachedOrCompute(cache, "analytics:by-source", () => reportService.GetLeadsBySource());
});

// What percentage of leads have been converted to customers
analytics.MapGet("/conversion-rate", async (ReportService reportService, IDistributedCache cache) =>
{
    return await GetCachedOrCompute(cache, "analytics:conversion-rate", () => reportService.GetConversionRate());
});

// How many leads are in each status (New, Contacted, Qualified, etc.)
analytics.MapGet("/by-status", async (ReportService reportService, IDistributedCache cache) =>
{
    return await GetCachedOrCompute(cache, "analytics:by-status", () => reportService.GetLeadStatusDistribution());
});

// How many leads each sales rep handles
analytics.MapGet("/by-salesrep", async (ReportService reportService, IDistributedCache cache) =>
{
    return await GetCachedOrCompute(cache, "analytics:by-salesrep", () => reportService.GetLeadsBySalesRep());
});

// ===== LEGACY REPORT ROUTES: Older URL format kept for backward compatibility with frontend =====
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

// ===== START THE APP: Begin listening for incoming HTTP requests =====
app.Run();

// ===== HELPER METHODS: Reusable functions used by the endpoints above =====

// Converts an OperationResult into the right HTTP response (200 OK, 400 Bad Request, or 404 Not Found)
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

// Checks Redis cache first — if data exists, return it; otherwise compute it and store in cache for 5 minutes
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

// Clears all analytics data from Redis cache (called whenever leads or interactions change)
static async Task InvalidateAnalyticsCache(IDistributedCache cache)
{
    var keys = new[] { "analytics:by-source", "analytics:conversion-rate", "analytics:by-status", "analytics:by-salesrep" };
    foreach (var key in keys)
        await cache.RemoveAsync(key);
}

// ===== RECORD TYPES: Simple data shapes used to read incoming request bodies =====

// The data needed to create a new lead
public sealed record CreateLeadRequest(
    string Name,
    string? Email,
    string? Phone,
    string? Company,
    string? Position,
    string? Status,
    string? Source,
    string? Priority,
    int? AssignedSalesRepId);

// The data needed to log in (email + password)
public sealed record LoginRequest(string Email, string Password);

// The data needed to update an existing lead
public sealed record UpdateLeadRequest(
    string Name,
    string? Email,
    string? Phone,
    string? Company,
    string? Position,
    string Status,
    string Source,
    string Priority,
    int? AssignedSalesRepId);

// The data needed to change a lead's status
public sealed record LeadStatusUpdateRequest(string NewStatus);

// The data needed to create a new interaction (phone call, meeting, email, etc.)
public sealed record CreateInteractionRequest(
    string InteractionType,
    string Notes,
    DateTime? InteractionDate,
    DateTime? FollowUpDate,
    int LeadId);

/*
 * FILE SUMMARY: Program.cs
 *
 * This is the main entry point for the Lead Management API. It configures everything
 * the application needs: database connection (SQL Server), authentication (JWT tokens),
 * caching (Redis), and service discovery (Consul). It defines all the API endpoints
 * for managing leads, interactions, and analytics reports. Every incoming HTTP request
 * flows through this file's middleware pipeline and gets routed to the right handler.
 */
