// This tells C# which folder/group this file belongs to
namespace LeadManagementSystem.Consul;

// This class holds all the settings needed to connect to and register with Consul
// (Consul is a tool that helps different services find each other on a network)
public class ConsulSettings
{
    // This is the name used to find these settings in the app's configuration file (appsettings.json)
    public const string SectionName = "Consul";

    // The web address where Consul is running (default: on this same machine, port 8500)
    public string Address { get; set; } = "http://localhost:8500";
    // The name our service will use when registering with Consul
    public string ServiceName { get; set; } = "lead-management-service";
    // A unique ID for this specific instance of our service
    public string ServiceId { get; set; } = "lead-management-service-5000";
    // The network address where our service can be reached
    public string ServiceAddress { get; set; } = "localhost";
    // The port number our service is listening on
    public int ServicePort { get; set; } = 5000;
    // The URL path Consul will ping to check if our service is still alive
    public string HealthCheckEndpoint { get; set; } = "/api/health";
    // How often (in seconds) Consul checks if our service is healthy
    public int HealthCheckIntervalSeconds { get; set; } = 10;
    // How long (in seconds) Consul waits for a health check response before giving up
    public int HealthCheckTimeoutSeconds { get; set; } = 5;
    // If the service is unhealthy for this many minutes, Consul removes it from the list
    public int DeregisterCriticalAfterMinutes { get; set; } = 1;
}

/*
 * FILE SUMMARY: ConsulSettings.cs
 * This file defines the configuration settings for connecting to Consul, a service discovery tool.
 * It stores values like the Consul server address, our service's name, port, and health check details.
 * These values are read from appsettings.json when the app starts and can be changed without modifying code.
 * This settings class is used by ConsulRegistrationHostedService to know how to register with Consul.
 */
