namespace LeadManagementSystem.Consul;

public class ConsulSettings
{
    public const string SectionName = "Consul";

    public string Address { get; set; } = "http://localhost:8500";
    public string ServiceName { get; set; } = "lead-management-service";
    public string ServiceId { get; set; } = "lead-management-service-5001";
    public string ServiceAddress { get; set; } = "localhost";
    public int ServicePort { get; set; } = 5001;
    public string HealthCheckEndpoint { get; set; } = "/api/health";
    public int HealthCheckIntervalSeconds { get; set; } = 10;
    public int HealthCheckTimeoutSeconds { get; set; } = 5;
    public int DeregisterCriticalAfterMinutes { get; set; } = 1;
}
