using Consul;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace LeadManagementSystem.Consul;

public class ConsulRegistrationHostedService : IHostedService
{
    private readonly IConsulClient _consulClient;
    private readonly IOptions<ConsulSettings> _settings;
    private readonly ILogger<ConsulRegistrationHostedService> _logger;

    public ConsulRegistrationHostedService(
        IConsulClient consulClient,
        IOptions<ConsulSettings> settings,
        ILogger<ConsulRegistrationHostedService> logger)
    {
        _consulClient = consulClient;
        _settings = settings;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
        var settings = _settings.Value;
        var serviceId = string.IsNullOrWhiteSpace(settings.ServiceId)
            ? $"{settings.ServiceName}-{settings.ServicePort}"
            : settings.ServiceId;

        await _consulClient.Agent.ServiceDeregister(serviceId, cancellationToken);

        var registration = new AgentServiceRegistration
        {
            ID = serviceId,
            Name = settings.ServiceName,
            Address = settings.ServiceAddress,
            Port = settings.ServicePort,
            Check = new AgentServiceCheck
            {
                HTTP = $"http://{settings.ServiceAddress}:{settings.ServicePort}{NormalizePath(settings.HealthCheckEndpoint)}",
                Interval = TimeSpan.FromSeconds(settings.HealthCheckIntervalSeconds),
                Timeout = TimeSpan.FromSeconds(settings.HealthCheckTimeoutSeconds),
                DeregisterCriticalServiceAfter = TimeSpan.FromMinutes(settings.DeregisterCriticalAfterMinutes)
            }
        };

        await _consulClient.Agent.ServiceRegister(registration, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Consul registration failed — running without service discovery");
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        var settings = _settings.Value;
        var serviceId = string.IsNullOrWhiteSpace(settings.ServiceId)
            ? $"{settings.ServiceName}-{settings.ServicePort}"
            : settings.ServiceId;

        await _consulClient.Agent.ServiceDeregister(serviceId, cancellationToken);
    }

    private static string NormalizePath(string endpoint)
    {
        if (string.IsNullOrWhiteSpace(endpoint))
        {
            return "/api/health";
        }

        return endpoint.StartsWith('/') ? endpoint : $"/{endpoint}";
    }
}
