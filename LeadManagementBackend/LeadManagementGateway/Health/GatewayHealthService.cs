// Import Stopwatch to measure how long backend health checks take
using System.Diagnostics;
// Import Consul discovery to find backend services
using LeadManagementGateway.Consul;
// Import SqlClient to test database connectivity
using Microsoft.Data.SqlClient;
// Import Options pattern for reading settings that can change at runtime
using Microsoft.Extensions.Options;

namespace LeadManagementGateway.Health;

// This service checks the health of the entire system: database, backend servers, and response times
public class GatewayHealthService
{
    // Reads app configuration (like reverse proxy settings)
    private readonly IConfiguration _configuration;
    // Creates HTTP clients for calling backend health endpoints
    private readonly IHttpClientFactory _httpClientFactory;
    // Reads health check settings (timeouts, thresholds, etc.)
    private readonly IOptionsMonitor<GatewayHealthSettings> _settings;
    // Discovers backend services through Consul
    private readonly ConsulDiscoveryService _consulDiscoveryService;

    // Constructor: .NET automatically provides all these dependencies
    public GatewayHealthService(
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory,
        IOptionsMonitor<GatewayHealthSettings> settings,
        ConsulDiscoveryService consulDiscoveryService)
    {
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
        _settings = settings;
        _consulDiscoveryService = consulDiscoveryService;
    }

    // Main method: runs all health checks and builds a full health report
    public async Task<GatewayHealthReport> CheckHealthAsync(CancellationToken cancellationToken)
    {
        var settings = _settings.CurrentValue;

        // Step 1: Check if the database is reachable
        var databaseStatus = await CheckDatabaseAsync(settings, cancellationToken);
        // Step 2: Get the list of backend server addresses
        var backendServers = await GetBackendServersAsync(cancellationToken);
        // Step 3: Ping each backend server to see if it's alive
        var backendChecks = await CheckBackendServersAsync(backendServers, settings, cancellationToken);

        // Filter to only the servers that are currently running
        var activeServers = backendChecks
            .Where(check => check.Status == "Running")
            .Select(check => check.Server)
            .ToList();

        // Get only the checks that succeeded and have a response time
        var successfulChecks = backendChecks
            .Where(check => check.Status == "Running" && check.ResponseTimeMs.HasValue)
            .ToList();

        // Calculate the average response time across all healthy backends
        var averageResponseMs = successfulChecks.Count == 0
            ? 0
            : successfulChecks.Average(check => check.ResponseTimeMs!.Value);

        // Format the response time as a string (e.g., "12.3ms") or "N/A" if no data
        var responseTime = successfulChecks.Count == 0 ? "N/A" : $"{Math.Round(averageResponseMs, 1)}ms";
        // Determine if the service is OK, Slow, or Down
        var customerService = GetServiceStatus(activeServers.Count, averageResponseMs, settings.SlowResponseThresholdMs);
        // Determine the overall system health: Healthy, Degraded, or Unhealthy
        var overallStatus = GetOverallStatus(databaseStatus, activeServers.Count, backendChecks, customerService);
        // Build a human-readable message explaining the health status
        var message = BuildMessage(databaseStatus, backendChecks, customerService);

        // Return the complete health report
        return new GatewayHealthReport(
            Status: overallStatus,
            Database: databaseStatus,
            CustomerService: customerService,
            ResponseTime: responseTime,
            ActiveServers: activeServers,
            Message: message,
            Backends: backendChecks,
            CheckedAtUtc: DateTime.UtcNow);
    }

    // Try to connect to the SQL Server database and run a simple query to see if it's alive
    private async Task<string> CheckDatabaseAsync(GatewayHealthSettings settings, CancellationToken cancellationToken)
    {
        // If no connection string is configured, we can't check the database
        if (string.IsNullOrWhiteSpace(settings.SqlConnectionString))
        {
            return "UNKNOWN";
        }

        try
        {
            // Open a connection and run "SELECT 1" as a quick test
            await using var connection = new SqlConnection(settings.SqlConnectionString);
            await connection.OpenAsync(cancellationToken);
            await using var command = new SqlCommand("SELECT 1", connection);
            await command.ExecuteScalarAsync(cancellationToken);
            return "UP";
        }
        catch
        {
            // If anything goes wrong, the database is considered down
            return "DOWN";
        }
    }

    // Get backend server addresses: first try Consul, then fall back to static config
    private async Task<List<string>> GetBackendServersAsync(CancellationToken cancellationToken)
    {
        // Try to discover servers dynamically via Consul
        var discoveredByConsul = await _consulDiscoveryService.GetHealthyServiceAddressesAsync(cancellationToken);
        if (discoveredByConsul.Count > 0)
        {
            return discoveredByConsul;
        }

        // If Consul has no results, read server addresses from the config file
        return GetBackendServersFromConfig();
    }

    // Read backend server addresses from the ReverseProxy section in appsettings.json
    private List<string> GetBackendServersFromConfig()
    {
        return _configuration.GetSection("ReverseProxy:Clusters")
            .GetChildren()
            .SelectMany(cluster => cluster.GetSection("Destinations").GetChildren())
            .Select(destination => destination.GetValue<string>("Address"))
            .Where(address => !string.IsNullOrWhiteSpace(address))
            .Select(address => EnsureTrailingSlash(address!))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    // Ping each backend server's health endpoint and measure response time
    private async Task<List<BackendServerHealth>> CheckBackendServersAsync(
        List<string> backendServers,
        GatewayHealthSettings settings,
        CancellationToken cancellationToken)
    {
        // Build the health check URL path (e.g., "/api/health")
        var healthPath = NormalizeHealthPath(settings.BackendHealthPath);
        // Use configured timeout, or default to 3 seconds
        var timeoutMs = settings.RequestTimeoutMs <= 0 ? 3000 : settings.RequestTimeoutMs;

        // Check all servers in parallel for speed
        var checks = backendServers.Select(async server =>
        {
            // Start a stopwatch to measure response time
            var watch = Stopwatch.StartNew();
            // Build the full URL to the server's health endpoint
            var requestUrl = new Uri(new Uri(server), healthPath.TrimStart('/'));
            var httpClient = _httpClientFactory.CreateClient();

            // Set a timeout so we don't wait forever for a dead server
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(TimeSpan.FromMilliseconds(timeoutMs));

            try
            {
                // Make an HTTP GET request to the health endpoint
                using var response = await httpClient.GetAsync(requestUrl, timeoutCts.Token);
                watch.Stop();

                // If the server returned a success status (200-299), it's running
                if (response.IsSuccessStatusCode)
                {
                    return new BackendServerHealth(server, "Running", watch.ElapsedMilliseconds, null);
                }

                // If the server returned an error status, mark it as down
                return new BackendServerHealth(
                    server,
                    "Down",
                    watch.ElapsedMilliseconds,
                    $"HTTP {(int)response.StatusCode}");
            }
            catch (Exception ex)
            {
                // If the request failed entirely (timeout, network error), mark as down
                watch.Stop();
                return new BackendServerHealth(server, "Down", watch.ElapsedMilliseconds, ex.Message);
            }
        });

        // Wait for all checks to finish and return the results
        return (await Task.WhenAll(checks)).ToList();
    }

    // Helper: make sure a URL ends with a slash
    private static string EnsureTrailingSlash(string address)
    {
        return address.EndsWith('/') ? address : $"{address}/";
    }

    // Helper: clean up the health path and ensure it starts with a slash
    private static string NormalizeHealthPath(string path)
    {
        var cleaned = (path ?? string.Empty).Trim().Trim('/');
        return string.IsNullOrWhiteSpace(cleaned) ? "/api/health" : $"/{cleaned}";
    }

    // Determine service status: "Down" if no servers, "Slow" if above threshold, otherwise "OK"
    private static string GetServiceStatus(int activeServerCount, double averageResponseMs, int thresholdMs)
    {
        if (activeServerCount == 0)
        {
            return "Down";
        }

        return averageResponseMs > thresholdMs ? "Slow" : "OK";
    }

    // Determine overall system health based on database, backends, and service status
    private static string GetOverallStatus(
        string databaseStatus,
        int activeServerCount,
        IReadOnlyCollection<BackendServerHealth> backendChecks,
        string customerService)
    {
        // If no backends are running, the system is unhealthy
        if (activeServerCount == 0)
        {
            return "Unhealthy";
        }

        // If database is down, some backends are down, or service is slow, it's degraded
        var hasDownBackend = backendChecks.Any(check => check.Status == "Down");
        if (databaseStatus == "DOWN" || hasDownBackend || customerService == "Slow")
        {
            return "Degraded";
        }

        return "Healthy";
    }

    // Build a human-readable message explaining any problems found
    private static string BuildMessage(
        string databaseStatus,
        IReadOnlyCollection<BackendServerHealth> backendChecks,
        string customerService)
    {
        var messages = new List<string>();

        if (databaseStatus == "DOWN")
        {
            messages.Add("Database connectivity failed");
        }

        var downCount = backendChecks.Count(check => check.Status == "Down");
        if (downCount > 0)
        {
            messages.Add($"{downCount} backend server(s) unavailable");
        }

        if (customerService == "Slow")
        {
            messages.Add("Customer service latency high");
        }

        // If no problems, return a positive message
        if (messages.Count == 0)
        {
            return "All systems operational";
        }

        return string.Join("; ", messages);
    }
}

// Data record that holds the health status of a single backend server
public sealed record BackendServerHealth(
    string Server,
    string Status,
    long? ResponseTimeMs,
    string? Error);

// Data record that holds the complete health report for the entire gateway
public sealed record GatewayHealthReport(
    string Status,
    string Database,
    string CustomerService,
    string ResponseTime,
    IReadOnlyList<string> ActiveServers,
    string Message,
    IReadOnlyList<BackendServerHealth> Backends,
    DateTime CheckedAtUtc);

/*
 * FILE SUMMARY:
 * This service performs health checks on the entire Lead Management system from the gateway's perspective.
 * It checks if the SQL Server database is reachable, discovers backend servers via Consul (or config), and pings each one.
 * Based on the results, it calculates an overall health status: Healthy, Degraded, or Unhealthy.
 * The health report includes response times, active servers, and human-readable status messages.
 * This is used by the /health and /gateway/health endpoints to monitor the system.
 */
