namespace LeadManagementGateway.Consul;

// This class holds configuration settings for connecting to Consul (a service discovery tool)
public class ConsulSettings
{
    // The name of the section in appsettings.json where these settings live
    public const string SectionName = "Consul";

    // The URL where the Consul server is running (default: localhost on port 8500)
    public string Address { get; set; } = "http://localhost:8500";
    // The list of microservice names to look up in Consul
    public List<string> ServiceNames { get; set; } = new() { "lead-management-service" };
}

/*
 * FILE SUMMARY:
 * This is a simple settings class that maps to the "Consul" section in appsettings.json.
 * It stores the Consul server address and the list of backend service names to discover.
 * The gateway reads these settings to know where Consul is and which services to look for.
 * This follows the .NET Options pattern for strongly-typed configuration.
 */
