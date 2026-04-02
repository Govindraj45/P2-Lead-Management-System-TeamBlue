// Import the Consul client library for service discovery
using Consul;
// Import hosting tools so this class runs automatically when the app starts and stops
using Microsoft.Extensions.Hosting;
// Import options pattern to read Consul settings from configuration
using Microsoft.Extensions.Options;

namespace LeadManagementSystem.Consul;

// This background service automatically registers and deregisters the microservice with Consul
// Consul is a "service discovery" tool — it keeps a directory of all running microservices
// so the API Gateway can find them
public class ConsulRegistrationHostedService : IHostedService
{
    // The Consul client used to talk to the Consul server
    private readonly IConsulClient _consulClient;
    // The configuration settings (service name, address, port, health check URL, etc.)
    private readonly IOptions<ConsulSettings> _settings;

    // Constructor: receives the Consul client and settings through dependency injection
    public ConsulRegistrationHostedService(
        IConsulClient consulClient,
        IOptions<ConsulSettings> settings)
    {
        _consulClient = consulClient;
        _settings = settings;
    }

    // This runs automatically when the application starts up
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var settings = _settings.Value;

        // Create a unique ID for this service instance (e.g., "leads-service-5001")
        var serviceId = string.IsNullOrWhiteSpace(settings.ServiceId)
            ? $"{settings.ServiceName}-{settings.ServicePort}"
            : settings.ServiceId;

        // Remove any old registration for this service (clean start)
        await _consulClient.Agent.ServiceDeregister(serviceId, cancellationToken);

        // Build the registration details: name, address, port, and health check
        var registration = new AgentServiceRegistration
        {
            ID = serviceId,
            Name = settings.ServiceName,
            Address = settings.ServiceAddress,
            Port = settings.ServicePort,
            // Tell Consul how to check if this service is still alive and healthy
            Check = new AgentServiceCheck
            {
                HTTP = $"http://{settings.ServiceAddress}:{settings.ServicePort}{NormalizePath(settings.HealthCheckEndpoint)}",
                Interval = TimeSpan.FromSeconds(settings.HealthCheckIntervalSeconds),
                Timeout = TimeSpan.FromSeconds(settings.HealthCheckTimeoutSeconds),
                DeregisterCriticalServiceAfter = TimeSpan.FromMinutes(settings.DeregisterCriticalAfterMinutes)
            }
        };

        // Register this microservice with Consul so the gateway can find it
        await _consulClient.Agent.ServiceRegister(registration, cancellationToken);
    }

    // This runs automatically when the application shuts down
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        var settings = _settings.Value;

        // Build the same service ID we used during registration
        var serviceId = string.IsNullOrWhiteSpace(settings.ServiceId)
            ? $"{settings.ServiceName}-{settings.ServicePort}"
            : settings.ServiceId;

        // Tell Consul this service is going offline — remove it from the directory
        await _consulClient.Agent.ServiceDeregister(serviceId, cancellationToken);
    }

    // Helper: make sure the health check path starts with "/" (e.g., "/api/health")
    private static string NormalizePath(string endpoint)
    {
        if (string.IsNullOrWhiteSpace(endpoint))
        {
            return "/api/health";
        }

        return endpoint.StartsWith('/') ? endpoint : $"/{endpoint}";
    }
}

/*
 * FILE SUMMARY — Consul/ConsulRegistrationHostedService.cs (Shared Library)
 * This file handles automatic service registration with Consul, a service discovery tool.
 * When a microservice starts, it registers itself (name, address, port, health check URL) so the
 * API Gateway knows where to route requests; when it stops, it deregisters itself.
 * As part of the shared library, every microservice (Leads, Interactions, Reports, SalesReps)
 * reuses this class to register with Consul without duplicating any code.
 */
