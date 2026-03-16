using Consul;
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

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection(MongoDbSettings.SectionName));

builder.Services.Configure<ConsulSettings>(
    builder.Configuration.GetSection(ConsulSettings.SectionName));

builder.Services.AddSingleton<IConsulClient>(_ =>
{
    var consulAddress = builder.Configuration[$"{ConsulSettings.SectionName}:Address"] ?? "http://localhost:8500";
    return new ConsulClient(config => { config.Address = new Uri(consulAddress); });
});
builder.Services.AddHostedService<ConsulRegistrationHostedService>();

builder.Services.AddSingleton<MongoDbContext>();
builder.Services.AddSingleton<MongoSequenceService>();

builder.Services.AddMediatR(typeof(Program).Assembly);

builder.Services.AddScoped<ILeadRepository, MongoLeadRepository>();
builder.Services.AddScoped<ISalesRepository, MongoSalesRepository>();
builder.Services.AddScoped<IInteractionRepository, MongoInteractionRepository>();
builder.Services.AddScoped<LeadService>();
builder.Services.AddScoped<ReportService>();

var app = builder.Build();

app.MapGet("/", () => Results.Ok(new
{
    service = "LeadManagementApp API",
    status = "running",
    database = "MongoDB",
    discovery = "Consul",
    architecture = "Mediator + MongoDB"
}));

app.MapGet("/api/health", () => Results.Ok(new
{
    service = "LeadManagementApp",
    status = "Healthy",
    utcTime = DateTime.UtcNow
}));

var leads = app.MapGroup("/api/leads");

leads.MapGet("", async (IMediator mediator) =>
{
    var allLeads = await mediator.Send(new GetAllLeadsQuery());
    return Results.Ok(allLeads);
});

leads.MapGet("/{id:int}", async (int id, IMediator mediator) =>
{
    var lead = await mediator.Send(new GetLeadByIdQuery(id));
    return lead is null ? Results.NotFound(new { message = "Lead not found." }) : Results.Ok(lead);
});

leads.MapPost("", async (CreateLeadRequest request, IMediator mediator) =>
{
    var result = await mediator.Send(new CreateLeadCommand(
        request.Name,
        request.Email,
        request.Phone,
        request.Company,
        request.Status,
        request.Source,
        request.Priority,
        request.AssignedToRepId));

    return result.Success
        ? Results.Created($"/api/leads/{result.Value}", new { id = result.Value, message = result.Message })
        : Results.BadRequest(new { message = result.Message });
});

leads.MapPut("/{id:int}", async (int id, UpdateLeadRequest request, IMediator mediator) =>
{
    var result = await mediator.Send(new UpdateLeadCommand(
        id,
        request.Name,
        request.Email,
        request.Phone,
        request.Company,
        request.Status,
        request.Source,
        request.Priority,
        request.AssignedToRepId));

    return ToHttpResult(result, missingResourceStatusCode: StatusCodes.Status404NotFound);
});

leads.MapDelete("/{id:int}", async (int id, IMediator mediator) =>
{
    var result = await mediator.Send(new DeleteLeadCommand(id));
    return ToHttpResult(result, missingResourceStatusCode: StatusCodes.Status404NotFound);
});

leads.MapPut("/{id:int}/status", async (int id, LeadStatusUpdateRequest request, IMediator mediator) =>
{
    var result = await mediator.Send(new UpdateLeadStatusCommand(id, request.NewStatus));
    return ToHttpResult(result, missingResourceStatusCode: StatusCodes.Status404NotFound);
});

leads.MapPost("/{id:int}/convert", async (int id, IMediator mediator) =>
{
    var result = await mediator.Send(new ConvertLeadToCustomerCommand(id));
    return ToHttpResult(result, missingResourceStatusCode: StatusCodes.Status404NotFound);
});

var reps = app.MapGroup("/api/reps");

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

var interactions = app.MapGroup("/api/interactions");

interactions.MapGet("/lead/{leadId:int}", async (int leadId, IMediator mediator) =>
{
    var items = await mediator.Send(new GetInteractionsByLeadQuery(leadId));
    return Results.Ok(items);
});

interactions.MapPost("", async (CreateInteractionRequest request, IMediator mediator) =>
{
    var result = await mediator.Send(new CreateInteractionCommand(
        request.InteractionType,
        request.Details,
        request.InteractionDate,
        request.FollowUpDate,
        request.LeadId));

    return result.Success
        ? Results.Created($"/api/interactions/{result.Value}", new { id = result.Value, message = result.Message })
        : Results.BadRequest(new { message = result.Message });
});

var reports = app.MapGroup("/api/reports");

reports.MapGet("/status-distribution", async (IMediator mediator) =>
{
    var distribution = await mediator.Send(new GetLeadStatusDistributionQuery());
    return Results.Ok(distribution);
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

public sealed record CreateLeadRequest(
    string Name,
    string? Email,
    string? Phone,
    string? Company,
    string? Status,
    string? Source,
    string? Priority,
    int? AssignedToRepId);

public sealed record UpdateLeadRequest(
    string Name,
    string? Email,
    string? Phone,
    string? Company,
    string Status,
    string Source,
    string Priority,
    int? AssignedToRepId);

public sealed record LeadStatusUpdateRequest(string NewStatus);

public sealed record CreateSalesRepRequest(string Name, string Email, string? Department);

public sealed record UpdateSalesRepRequest(string Name, string Email, string Department);

public sealed record CreateInteractionRequest(
    string InteractionType,
    string Details,
    DateTime? InteractionDate,
    DateTime? FollowUpDate,
    int LeadId);
