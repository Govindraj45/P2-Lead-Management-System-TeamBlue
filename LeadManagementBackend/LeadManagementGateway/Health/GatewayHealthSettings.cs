namespace LeadManagementGateway.Health;

public class GatewayHealthSettings
{
    public const string SectionName = "GatewayHealth";

    public string SqlConnectionString { get; set; } = "Server=localhost;Database=CRM_LeadManagement;TrustServerCertificate=True";
    public string BackendHealthPath { get; set; } = "api/health";
    public int SlowResponseThresholdMs { get; set; } = 10;
    public int RequestTimeoutMs { get; set; } = 3000;
}
