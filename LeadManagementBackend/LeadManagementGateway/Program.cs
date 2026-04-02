// Import the Consul library for service discovery
using Consul;
// Import custom classes for Consul settings and health checks
using LeadManagementGateway.Consul;
using LeadManagementGateway.Health;

// Create the web application builder — this is the starting point of the app
var builder = WebApplication.CreateBuilder(args);

// Read Consul settings from the configuration file (appsettings.json)
builder.Services.Configure<ConsulSettings>(
    builder.Configuration.GetSection(ConsulSettings.SectionName));
// Read health check settings from the configuration file
builder.Services.Configure<GatewayHealthSettings>(
    builder.Configuration.GetSection(GatewayHealthSettings.SectionName));

// Register the Consul client so the gateway can talk to the Consul service registry
builder.Services.AddSingleton<IConsulClient>(_ =>
{
    // Get the Consul server address from config, or default to localhost
    var consulAddress = builder.Configuration[$"{ConsulSettings.SectionName}:Address"] ?? "http://localhost:8500";
    return new ConsulClient(config => { config.Address = new Uri(consulAddress); });
});
// Register HttpClient so we can make HTTP calls to backend services
builder.Services.AddHttpClient();
// Register the service that discovers backend services via Consul
builder.Services.AddSingleton<ConsulDiscoveryService>();
// Register the service that checks the health of backend services
builder.Services.AddSingleton<GatewayHealthService>();

// Set up YARP (Yet Another Reverse Proxy) to forward requests to backend microservices
builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Build the application with all the registered services
var app = builder.Build();

// When someone visits the root URL "/", show gateway info and available routes
app.MapGet("/", (IConfiguration configuration) =>
{
    // Read all route names from the reverse proxy config
    var routes = configuration.GetSection("ReverseProxy:Routes")
        .GetChildren()
        .Select(route => route.Key)
        .ToArray();

    // Return a JSON response with gateway name, status, and routes
    return Results.Ok(new
    {
        gateway = "Lead Management API Gateway",
        status = "running",
        availableRoutes = routes
    });
});

// Health check endpoint at "/health" — returns the health status of the whole system
app.MapGet("/health", async (GatewayHealthService healthService, CancellationToken cancellationToken) =>
{
    // Run health checks on database, backend servers, etc.
    var report = await healthService.CheckHealthAsync(cancellationToken);
    // Return 503 if unhealthy, 200 if healthy
    var statusCode = report.Status == "Unhealthy" ? StatusCodes.Status503ServiceUnavailable : StatusCodes.Status200OK;
    return Results.Json(report, statusCode: statusCode);
});

// Alternative health check endpoint at "/gateway/health" (same logic as "/health")
app.MapGet("/gateway/health", async (GatewayHealthService healthService, CancellationToken cancellationToken) =>
{
    var report = await healthService.CheckHealthAsync(cancellationToken);
    var statusCode = report.Status == "Unhealthy" ? StatusCodes.Status503ServiceUnavailable : StatusCodes.Status200OK;
    return Results.Json(report, statusCode: statusCode);
});

// Endpoint to list all healthy services discovered through Consul
app.MapGet("/gateway/consul/services", async (ConsulDiscoveryService discoveryService, CancellationToken cancellationToken) =>
{
    // Ask Consul for all healthy backend service addresses
    var services = await discoveryService.GetHealthyServiceAddressesAsync(cancellationToken);
    return Results.Ok(new
    {
        source = "Consul",
        count = services.Count,
        services
    });
});

// Enable the reverse proxy so the gateway forwards requests to backend services
app.MapReverseProxy();

// Start the gateway and begin listening for incoming requests
app.Run();

/*
 * FILE SUMMARY:
 * This is the main entry point for the API Gateway service.
 * It sets up a reverse proxy (YARP) that forwards incoming HTTP requests to the correct backend microservices.
 * It registers health check endpoints so we can monitor if the database and backend servers are running.
 * The gateway uses Consul for service discovery, meaning it can automatically find backend services.
 * This file configures all the services, middleware, and routes needed to run the gateway.
 */
