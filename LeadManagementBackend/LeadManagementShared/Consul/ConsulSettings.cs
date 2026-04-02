namespace LeadManagementSystem.Consul;

// This class holds all the configuration settings needed to register with Consul
// These values come from appsettings.json under the "Consul" section
public class ConsulSettings
{
    // The name of the config section in appsettings.json (used to bind settings automatically)
    public const string SectionName = "Consul";

    // The URL of the Consul server (where the service directory lives)
    public string Address { get; set; } = "http://localhost:8500";

    // The name this microservice will be registered under in Consul
    public string ServiceName { get; set; } = "lead-management-service";

    // A unique ID for this specific instance of the service
    public string ServiceId { get; set; } = "lead-management-service-5000";

    // The hostname or IP address where this service is running
    public string ServiceAddress { get; set; } = "localhost";

    // The port number this service listens on
    public int ServicePort { get; set; } = 5000;

    // The URL path Consul will call to check if this service is healthy
    public string HealthCheckEndpoint { get; set; } = "/api/health";

    // How often (in seconds) Consul pings the health check endpoint
    public int HealthCheckIntervalSeconds { get; set; } = 10;

    // How long (in seconds) Consul waits for a health check response before giving up
    public int HealthCheckTimeoutSeconds { get; set; } = 5;

    // If the service is unhealthy for this many minutes, Consul removes it automatically
    public int DeregisterCriticalAfterMinutes { get; set; } = 1;
}

/*
 * FILE SUMMARY — Consul/ConsulSettings.cs (Shared Library)
 * This file defines a plain C# class that maps to the "Consul" section in appsettings.json.
 * It holds all the settings a microservice needs to register with Consul: the Consul server address,
 * the service's name/ID/address/port, and health check configuration.
 * As part of the shared library, every microservice reads its own Consul settings into this class
 * and passes them to the ConsulRegistrationHostedService for automatic registration.
 */
