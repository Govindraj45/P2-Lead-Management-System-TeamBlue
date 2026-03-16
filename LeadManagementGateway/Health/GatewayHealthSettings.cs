namespace LeadManagementGateway.Health;

public class GatewayHealthSettings
{
    public const string SectionName = "GatewayHealth";

    public string MongoConnectionString { get; set; } = "mongodb://localhost:27017";
    public string MongoDatabaseName { get; set; } = "LeadManagementSystem_Dev";
    public string BackendHealthPath { get; set; } = "api/health";
    public int SlowResponseThresholdMs { get; set; } = 10;
    public int RequestTimeoutMs { get; set; } = 3000;
}
