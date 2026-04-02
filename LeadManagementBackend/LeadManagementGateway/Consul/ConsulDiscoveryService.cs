using Consul;
using Microsoft.Extensions.Options;

namespace LeadManagementGateway.Consul;

public class ConsulDiscoveryService
{
    private readonly IConsulClient _consulClient;
    private readonly IOptionsMonitor<ConsulSettings> _settings;

    public ConsulDiscoveryService(
        IConsulClient consulClient,
        IOptionsMonitor<ConsulSettings> settings)
    {
        _consulClient = consulClient;
        _settings = settings;
    }

    public async Task<List<string>> GetHealthyServiceAddressesAsync(CancellationToken cancellationToken)
    {
        var settings = _settings.CurrentValue;
        var serviceNames = settings.ServiceNames
            ?.Where(name => !string.IsNullOrWhiteSpace(name))
            .Select(name => name.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList() ?? new List<string>();

        if (serviceNames.Count == 0)
        {
            return new List<string>();
        }

        var discovered = new List<string>();

        foreach (var serviceName in serviceNames)
        {
            try
            {
                var queryResult = await _consulClient.Health.Service(serviceName, tag: null, passingOnly: true, cancellationToken);
                foreach (var entry in queryResult.Response)
                {
                    var address = string.IsNullOrWhiteSpace(entry.Service.Address)
                        ? entry.Node.Address
                        : entry.Service.Address;

                    if (string.IsNullOrWhiteSpace(address) || entry.Service.Port <= 0)
                    {
                        continue;
                    }

                    discovered.Add($"http://{address}:{entry.Service.Port}/");
                }
            }
            catch
            {
                // Fallback handled by caller (static reverse-proxy config).
            }
        }

        return discovered
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
