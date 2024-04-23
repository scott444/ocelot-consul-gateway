namespace MinimalApi;

public class ConsulConfig
{
    public string ConsulHost { get; set; }
    public string ConsulPort { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public string ServiceId { get; set; } = string.Empty;
}
