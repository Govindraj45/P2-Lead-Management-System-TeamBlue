// Import the Consul client library for talking to the Consul service registry
using Consul;
// Import Options pattern so we can read settings that can change at runtime
using Microsoft.Extensions.Options;

namespace LeadManagementGateway.Consul;

// This service asks Consul "which backend servers are healthy and available?"
public class ConsulDiscoveryService
{
    // The client we use to communicate with Consul
    private readonly IConsulClient _consulClient;
    // Settings that tell us which service names to look up in Consul
    private readonly IOptionsMonitor<ConsulSettings> _settings;

    // Constructor: .NET automatically provides these dependencies (dependency injection)
    public ConsulDiscoveryService(
        IConsulClient consulClient,
        IOptionsMonitor<ConsulSettings> settings)
    {
        _consulClient = consulClient;
        _settings = settings;
    }

    // Find all healthy backend service addresses registered in Consul
    public async Task<List<string>> GetHealthyServiceAddressesAsync(CancellationToken cancellationToken)
    {
        // Get the current settings (service names to look up)
        var settings = _settings.CurrentValue;
        // Clean up the list of service names: remove blanks, trim spaces, remove duplicates
        var serviceNames = settings.ServiceNames
            ?.Where(name => !string.IsNullOrWhiteSpace(name))
            .Select(name => name.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList() ?? new List<string>();

        // If there are no service names to look up, return an empty list
        if (serviceNames.Count == 0)
        {
            return new List<string>();
        }

        // This list will hold all the discovered server addresses
        var discovered = new List<string>();

        // Loop through each service name and ask Consul for its healthy instances
        foreach (var serviceName in serviceNames)
        {
            try
            {
                // Query Consul for only healthy (passing) instances of this service
                var queryResult = await _consulClient.Health.Service(serviceName, tag: null, passingOnly: true, cancellationToken);
                // For each healthy instance, extract the address and port
                foreach (var entry in queryResult.Response)
                {
                    // Use the service address if available, otherwise fall back to the node address
                    var address = string.IsNullOrWhiteSpace(entry.Service.Address)
                        ? entry.Node.Address
                        : entry.Service.Address;

                    // Skip entries with missing address or invalid port
                    if (string.IsNullOrWhiteSpace(address) || entry.Service.Port <= 0)
                    {
                        continue;
                    }

                    // Build a full URL and add it to the list
                    discovered.Add($"http://{address}:{entry.Service.Port}/");
                }
            }
            catch
            {
                // If Consul is unavailable, the caller will use static config as a fallback
            }
        }

        // Remove duplicate addresses and return the final list
        return discovered
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}

/*
 * FILE SUMMARY:
 * This service discovers healthy backend microservices by querying the Consul service registry.
 * It reads a list of service names from configuration and asks Consul which instances are currently healthy.
 * For each healthy instance, it builds a URL (like "http://10.0.0.5:5001/") and returns all of them.
 * If Consul is unavailable, the error is silently caught so the gateway can fall back to static configuration.
 * This enables dynamic service discovery instead of hardcoding backend server addresses.
 */
