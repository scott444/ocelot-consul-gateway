namespace Infrastructure.ServiceDiscovery;

public class ServiceConfigOptions
{
    public const string ServiceConfig = "ServiceConfig";

    public string ServiceDiscoveryAddress { get; set; } = string.Empty;
    public bool UseServiceSettings { get; set; }
    public string ServiceId { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public string ServiceHost { get; set; } = string.Empty;
    public int ServicePort { get; set; } = 0;
    public string HealthCheckUrlSegment { get; set; } = string.Empty;
    public int HealthCheckIntervalSeconds { get; set; } = 10;
    public int HealthCheckTimeoutSeconds { get; set; } = 5;
}