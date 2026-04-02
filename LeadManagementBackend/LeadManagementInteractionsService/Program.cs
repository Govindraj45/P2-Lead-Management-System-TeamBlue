// Import libraries for Consul (service discovery), database, and interaction features
using Consul;
using LeadManagementSystem.Consul;
using LeadManagementSystem.Data;
using LeadManagementSystem.Features.Interactions;
using LeadManagementSystem.Interfaces;
using Microsoft.EntityFrameworkCore;

// Create the web application builder — this is the starting point of the app
var builder = WebApplication.CreateBuilder(args);

// Set up the database connection using SQL Server and a connection string from settings
builder.Services.AddDbContext<LeadDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Load Consul settings from the configuration file (used for service discovery)
builder.Services.Configure<ConsulSettings>(
    builder.Configuration.GetSection(ConsulSettings.SectionName));

// Register the Consul client so this service can register itself for discovery by other services
builder.Services.AddSingleton<IConsulClient>(_ =>
{
    var consulAddress = builder.Configuration[$"{ConsulSettings.SectionName}:Address"] ?? "http://localhost:8500";
    return new ConsulClient(config => { config.Address = new Uri(consulAddress); });
});
// Start a background task that registers this service with Consul when the app starts
builder.Services.AddHostedService<ConsulRegistrationHostedService>();

// Register CQRS command and query handlers (simple service-based, no MediatR)
builder.Services.AddScoped<CreateInteractionHandler>();
builder.Services.AddScoped<GetInteractionsByLeadHandler>();
// Add support for API controllers (classes that handle HTTP requests)
builder.Services.AddControllers();

// Tell the app which concrete classes to use for the repository interfaces (dependency injection)
builder.Services.AddScoped<IInteractionRepository, EfInteractionRepository>();
builder.Services.AddScoped<ILeadRepository, EfLeadRepository>();

// Build the app with all the services configured above
var app = builder.Build();

// Root endpoint — returns basic info about this service when you visit the base URL
app.MapGet("/", () => Results.Ok(new
{
    service = "LeadManagementInteractionsService",
    status = "running",
    database = "SQL Server"
}));

// Health check endpoint — used by monitoring tools to verify the service is alive
app.MapGet("/api/health", () => Results.Ok(new
{
    service = "LeadManagementInteractionsService",
    status = "Healthy",
    utcTime = DateTime.UtcNow
}));

// Map all controller routes so incoming HTTP requests reach the right controller methods
app.MapControllers();

// Start the web server and begin listening for requests
app.Run();

/*
    FILE SUMMARY:
    This is the startup file for the Interactions microservice.
    It configures the database connection, registers Consul for service discovery,
    and sets up dependency injection for CQRS handlers and repositories.
    It also defines a root info endpoint and a health check endpoint.
    Finally, it maps the API controllers and starts the web server.
*/
