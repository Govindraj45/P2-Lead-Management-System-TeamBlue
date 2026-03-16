using System.Diagnostics;
using LeadManagementGateway.Consul;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace LeadManagementGateway.Health;

public class GatewayHealthService
{
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IOptionsMonitor<GatewayHealthSettings> _settings;
    private readonly ConsulDiscoveryService _consulDiscoveryService;

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

    public async Task<GatewayHealthReport> CheckHealthAsync(CancellationToken cancellationToken)
    {
        var settings = _settings.CurrentValue;

        var databaseStatus = await CheckDatabaseAsync(settings, cancellationToken);
        var backendServers = await GetBackendServersAsync(cancellationToken);
        var backendChecks = await CheckBackendServersAsync(backendServers, settings, cancellationToken);

        var activeServers = backendChecks
            .Where(check => check.Status == "Running")
            .Select(check => check.Server)
            .ToList();

        var successfulChecks = backendChecks
            .Where(check => check.Status == "Running" && check.ResponseTimeMs.HasValue)
            .ToList();

        var averageResponseMs = successfulChecks.Count == 0
            ? 0
            : successfulChecks.Average(check => check.ResponseTimeMs!.Value);

        var responseTime = successfulChecks.Count == 0 ? "N/A" : $"{Math.Round(averageResponseMs, 1)}ms";
        var customerService = GetServiceStatus(activeServers.Count, averageResponseMs, settings.SlowResponseThresholdMs);
        var overallStatus = GetOverallStatus(databaseStatus, activeServers.Count, backendChecks, customerService);
        var message = BuildMessage(databaseStatus, backendChecks, customerService);

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

    private async Task<string> CheckDatabaseAsync(GatewayHealthSettings settings, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(settings.MongoConnectionString) ||
            string.IsNullOrWhiteSpace(settings.MongoDatabaseName))
        {
            return "UNKNOWN";
        }

        try
        {
            var client = new MongoClient(settings.MongoConnectionString);
            var database = client.GetDatabase(settings.MongoDatabaseName);
            await database.RunCommandAsync<BsonDocument>(new BsonDocument("ping", 1), cancellationToken: cancellationToken);
            return "UP";
        }
        catch
        {
            return "DOWN";
        }
    }

    private async Task<List<string>> GetBackendServersAsync(CancellationToken cancellationToken)
    {
        var discoveredByConsul = await _consulDiscoveryService.GetHealthyServiceAddressesAsync(cancellationToken);
        if (discoveredByConsul.Count > 0)
        {
            return discoveredByConsul;
        }

        return GetBackendServersFromConfig();
    }

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

    private async Task<List<BackendServerHealth>> CheckBackendServersAsync(
        List<string> backendServers,
        GatewayHealthSettings settings,
        CancellationToken cancellationToken)
    {
        var healthPath = NormalizeHealthPath(settings.BackendHealthPath);
        var timeoutMs = settings.RequestTimeoutMs <= 0 ? 3000 : settings.RequestTimeoutMs;

        var checks = backendServers.Select(async server =>
        {
            var watch = Stopwatch.StartNew();
            var requestUrl = new Uri(new Uri(server), healthPath.TrimStart('/'));
            var httpClient = _httpClientFactory.CreateClient();

            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(TimeSpan.FromMilliseconds(timeoutMs));

            try
            {
                using var response = await httpClient.GetAsync(requestUrl, timeoutCts.Token);
                watch.Stop();

                if (response.IsSuccessStatusCode)
                {
                    return new BackendServerHealth(server, "Running", watch.ElapsedMilliseconds, null);
                }

                return new BackendServerHealth(
                    server,
                    "Down",
                    watch.ElapsedMilliseconds,
                    $"HTTP {(int)response.StatusCode}");
            }
            catch (Exception ex)
            {
                watch.Stop();
                return new BackendServerHealth(server, "Down", watch.ElapsedMilliseconds, ex.Message);
            }
        });

        return (await Task.WhenAll(checks)).ToList();
    }

    private static string EnsureTrailingSlash(string address)
    {
        return address.EndsWith('/') ? address : $"{address}/";
    }

    private static string NormalizeHealthPath(string path)
    {
        var cleaned = (path ?? string.Empty).Trim().Trim('/');
        return string.IsNullOrWhiteSpace(cleaned) ? "/api/health" : $"/{cleaned}";
    }

    private static string GetServiceStatus(int activeServerCount, double averageResponseMs, int thresholdMs)
    {
        if (activeServerCount == 0)
        {
            return "Down";
        }

        return averageResponseMs > thresholdMs ? "Slow" : "OK";
    }

    private static string GetOverallStatus(
        string databaseStatus,
        int activeServerCount,
        IReadOnlyCollection<BackendServerHealth> backendChecks,
        string customerService)
    {
        if (activeServerCount == 0)
        {
            return "Unhealthy";
        }

        var hasDownBackend = backendChecks.Any(check => check.Status == "Down");
        if (databaseStatus == "DOWN" || hasDownBackend || customerService == "Slow")
        {
            return "Degraded";
        }

        return "Healthy";
    }

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

        if (messages.Count == 0)
        {
            return "All systems operational";
        }

        return string.Join("; ", messages);
    }
}

public sealed record BackendServerHealth(
    string Server,
    string Status,
    long? ResponseTimeMs,
    string? Error);

public sealed record GatewayHealthReport(
    string Status,
    string Database,
    string CustomerService,
    string ResponseTime,
    IReadOnlyList<string> ActiveServers,
    string Message,
    IReadOnlyList<BackendServerHealth> Backends,
    DateTime CheckedAtUtc);
