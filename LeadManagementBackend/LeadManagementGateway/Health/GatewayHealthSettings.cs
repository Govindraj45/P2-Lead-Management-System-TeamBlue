namespace LeadManagementGateway.Health;

// This class holds configuration settings for the gateway's health checking feature
public class GatewayHealthSettings
{
    // The name of the section in appsettings.json where these settings live
    public const string SectionName = "GatewayHealth";

    // The database connection string used to check if SQL Server is reachable
    public string SqlConnectionString { get; set; } = "Server=localhost;Database=CRM_LeadManagement;TrustServerCertificate=True";
    // The URL path on backend services to call for health checks (e.g., "api/health")
    public string BackendHealthPath { get; set; } = "api/health";
    // If a backend responds slower than this (in milliseconds), it is considered "Slow"
    public int SlowResponseThresholdMs { get; set; } = 10;
    // How long to wait (in milliseconds) before giving up on a backend health check
    public int RequestTimeoutMs { get; set; } = 3000;
}

/*
 * FILE SUMMARY:
 * This is a simple settings class that maps to the "GatewayHealth" section in appsettings.json.
 * It stores the database connection string, backend health check path, and timeout thresholds.
 * The GatewayHealthService reads these values to decide how to check and report system health.
 * This follows the .NET Options pattern for strongly-typed configuration.
 */
