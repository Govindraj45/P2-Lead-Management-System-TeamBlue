using Consul;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace LeadManagementSystem.Consul;

public class ConsulRegistrationHostedService : IHostedService
{
    private readonly IConsulClient _consulClient;
    private readonly IOptions<ConsulSettings> _settings;

    public ConsulRegistrationHostedService(
        IConsulClient consulClient,
        IOptions<ConsulSettings> settings)
    {
        _consulClient = consulClient;
        _settings = settings;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
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
