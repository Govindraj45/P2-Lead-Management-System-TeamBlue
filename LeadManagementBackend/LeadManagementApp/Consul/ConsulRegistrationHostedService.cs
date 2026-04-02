// Bring in the tools we need to talk to Consul (a service discovery tool)
using Consul;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

// This tells C# which folder/group this file belongs to
namespace LeadManagementSystem.Consul;

// This class registers our app with Consul when it starts, and unregisters when it stops
// IHostedService means this runs automatically in the background when the app starts/stops
public class ConsulRegistrationHostedService : IHostedService
{
    // _consulClient is our connection to the Consul server
    private readonly IConsulClient _consulClient;
    // _settings holds the Consul configuration (service name, address, port, etc.)
    private readonly IOptions<ConsulSettings> _settings;
    // _logger writes messages so we can see what happened
    private readonly ILogger<ConsulRegistrationHostedService> _logger;

    // Constructor — stores the tools we need for Consul registration
    public ConsulRegistrationHostedService(
        IConsulClient consulClient,
        IOptions<ConsulSettings> settings,
        ILogger<ConsulRegistrationHostedService> logger)
    {
        _consulClient = consulClient;
        _settings = settings;
        _logger = logger;
    }

    // This runs automatically when the app starts — it registers our service with Consul
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
        // Read the settings from configuration
        var settings = _settings.Value;
        // Create a unique ID for our service (either from settings or by combining name + port)
        var serviceId = string.IsNullOrWhiteSpace(settings.ServiceId)
            ? $"{settings.ServiceName}-{settings.ServicePort}"
            : settings.ServiceId;

        // Remove any old registration for this service (clean slate)
        await _consulClient.Agent.ServiceDeregister(serviceId, cancellationToken);

        // Set up the registration details — tells Consul where our service lives and how to check if it's healthy
        var registration = new AgentServiceRegistration
        {
            ID = serviceId,
            Name = settings.ServiceName,
            Address = settings.ServiceAddress,
            Port = settings.ServicePort,
            // The health check tells Consul to periodically ping our app to make sure it's still running
            Check = new AgentServiceCheck
            {
                HTTP = $"http://{settings.ServiceAddress}:{settings.ServicePort}{NormalizePath(settings.HealthCheckEndpoint)}",
                Interval = TimeSpan.FromSeconds(settings.HealthCheckIntervalSeconds),
                Timeout = TimeSpan.FromSeconds(settings.HealthCheckTimeoutSeconds),
                // If the service is down for too long, Consul will automatically remove it
                DeregisterCriticalServiceAfter = TimeSpan.FromMinutes(settings.DeregisterCriticalAfterMinutes)
            }
        };

        // Send the registration to Consul
        await _consulClient.Agent.ServiceRegister(registration, cancellationToken);
        }
        catch (Exception ex)
        {
            // If Consul isn't available, log a warning but keep the app running anyway
            _logger.LogWarning(ex, "Consul registration failed — running without service discovery");
        }
    }

    // This runs automatically when the app shuts down — it tells Consul we're going offline
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        var settings = _settings.Value;
        var serviceId = string.IsNullOrWhiteSpace(settings.ServiceId)
            ? $"{settings.ServiceName}-{settings.ServicePort}"
            : settings.ServiceId;

        // Tell Consul to remove our service from its list
        await _consulClient.Agent.ServiceDeregister(serviceId, cancellationToken);
    }

    // Helper method that makes sure the health check URL path starts with a forward slash
    private static string NormalizePath(string endpoint)
    {
        // If no endpoint was set, use a default health check path
        if (string.IsNullOrWhiteSpace(endpoint))
        {
            return "/api/health";
        }

        // Add a leading slash if one is missing
        return endpoint.StartsWith('/') ? endpoint : $"/{endpoint}";
    }
}

/*
 * FILE SUMMARY: ConsulRegistrationHostedService.cs
 * This file handles registering and unregistering our app with Consul, a service discovery tool.
 * When the app starts, it tells Consul "I'm here at this address and port" so other services can find it.
 * It also sets up a health check so Consul periodically pings our app to make sure it's still running.
 * When the app shuts down, it tells Consul to remove it from the list of available services.
 * If Consul isn't available, the app still works — it just runs without service discovery.
 */
