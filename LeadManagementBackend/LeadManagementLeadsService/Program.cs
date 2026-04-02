// Import the Consul library for service discovery
using Consul;
// Import custom classes for Consul registration and database access
using LeadManagementSystem.Consul;
using LeadManagementSystem.Data;
// Import all CQRS command and query handlers for leads
using LeadManagementSystem.Features.Leads;
// Import interfaces and business logic
using LeadManagementSystem.Interfaces;
using LeadManagementSystem.Logic;
// Import Entity Framework Core for database operations
using Microsoft.EntityFrameworkCore;

// Create the web application builder — this is the starting point of the service
var builder = WebApplication.CreateBuilder(args);

// Register the database context with SQL Server connection string from appsettings.json
builder.Services.AddDbContext<LeadDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Read Consul settings from the configuration file
builder.Services.Configure<ConsulSettings>(
    builder.Configuration.GetSection(ConsulSettings.SectionName));

// Register the Consul client so this service can register itself with Consul
builder.Services.AddSingleton<IConsulClient>(_ =>
{
    var consulAddress = builder.Configuration[$"{ConsulSettings.SectionName}:Address"] ?? "http://localhost:8500";
    return new ConsulClient(config => { config.Address = new Uri(consulAddress); });
});
// Start a background service that registers this microservice with Consul on startup
builder.Services.AddHostedService<ConsulRegistrationHostedService>();

// CQRS Handlers (simple service-based, no MediatR)
// Register each command/query handler so they can be injected into controllers
builder.Services.AddScoped<CreateLeadHandler>();
builder.Services.AddScoped<GetAllLeadsHandler>();
builder.Services.AddScoped<GetLeadByIdHandler>();
builder.Services.AddScoped<UpdateLeadHandler>();
builder.Services.AddScoped<UpdateLeadStatusHandler>();
builder.Services.AddScoped<DeleteLeadHandler>();
builder.Services.AddScoped<ConvertLeadToCustomerHandler>();
// Register controllers so the app can handle API requests
builder.Services.AddControllers();

// Register the repository (data access) and business logic service
builder.Services.AddScoped<ILeadRepository, EfLeadRepository>();
builder.Services.AddScoped<LeadService>();

// Build the application with all the registered services
var app = builder.Build();

// Root endpoint "/" — returns basic info about this service
app.MapGet("/", () => Results.Ok(new
{
    service = "LeadManagementLeadsService",
    status = "running",
    database = "SQL Server"
}));

// Health check endpoint "/api/health" — returns the health status of this service
app.MapGet("/api/health", () => Results.Ok(new
{
    service = "LeadManagementLeadsService",
    status = "Healthy",
    utcTime = DateTime.UtcNow
}));

// Map all controller routes (like /api/leads) to their handlers
app.MapControllers();

// Start the service and begin listening for requests
app.Run();

/*
 * FILE SUMMARY:
 * This is the main entry point for the Leads microservice, which handles all lead-related operations.
 * It sets up the database connection (SQL Server via EF Core), registers CQRS handlers for create/read/update/delete operations,
 * and registers this service with Consul for discovery by the API Gateway.
 * It also exposes a health check endpoint and maps all API controllers.
 * This service is one of several microservices that together make up the Lead Management System.
 */
