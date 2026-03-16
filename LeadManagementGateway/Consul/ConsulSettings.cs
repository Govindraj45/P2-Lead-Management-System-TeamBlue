namespace LeadManagementGateway.Consul;

public class ConsulSettings
{
    public const string SectionName = "Consul";

    public string Address { get; set; } = "http://localhost:8500";
    public List<string> ServiceNames { get; set; } = new() { "lead-management-service" };
}
