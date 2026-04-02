using Consul;
using LeadManagementGateway.Consul;
using LeadManagementGateway.Health;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ConsulSettings>(
    builder.Configuration.GetSection(ConsulSettings.SectionName));
builder.Services.Configure<GatewayHealthSettings>(
    builder.Configuration.GetSection(GatewayHealthSettings.SectionName));

builder.Services.AddSingleton<IConsulClient>(_ =>
{
    var consulAddress = builder.Configuration[$"{ConsulSettings.SectionName}:Address"] ?? "http://localhost:8500";
    return new ConsulClient(config => { config.Address = new Uri(consulAddress); });
});
builder.Services.AddHttpClient();
builder.Services.AddSingleton<ConsulDiscoveryService>();
builder.Services.AddSingleton<GatewayHealthService>();

builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

app.MapGet("/", (IConfiguration configuration) =>
{
    var routes = configuration.GetSection("ReverseProxy:Routes")
        .GetChildren()
        .Select(route => route.Key)
        .ToArray();

    return Results.Ok(new
    {
        gateway = "Lead Management API Gateway",
        status = "running",
        availableRoutes = routes
    });
});

app.MapGet("/health", async (GatewayHealthService healthService, CancellationToken cancellationToken) =>
{
    var report = await healthService.CheckHealthAsync(cancellationToken);
    var statusCode = report.Status == "Unhealthy" ? StatusCodes.Status503ServiceUnavailable : StatusCodes.Status200OK;
    return Results.Json(report, statusCode: statusCode);
});

app.MapGet("/gateway/health", async (GatewayHealthService healthService, CancellationToken cancellationToken) =>
{
    var report = await healthService.CheckHealthAsync(cancellationToken);
    var statusCode = report.Status == "Unhealthy" ? StatusCodes.Status503ServiceUnavailable : StatusCodes.Status200OK;
    return Results.Json(report, statusCode: statusCode);
});

app.MapGet("/gateway/consul/services", async (ConsulDiscoveryService discoveryService, CancellationToken cancellationToken) =>
{
    var services = await discoveryService.GetHealthyServiceAddressesAsync(cancellationToken);
    return Results.Ok(new
    {
        source = "Consul",
        count = services.Count,
        services
    });
});

app.MapReverseProxy();

app.Run();
