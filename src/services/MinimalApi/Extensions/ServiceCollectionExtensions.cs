using Consul;
using System.Net;


namespace MinimalApi.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddConsulConfig(this IServiceCollection services, string configKey)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<IConsulClient>(consul => new ConsulClient(consulConfig =>
        {
            consulConfig.Address = new Uri(configKey);
        }));

        return services;
    }
}

public static class ConsulExtensions
{
    public static void RegisterConsulService(this WebApplication app)
    {
        var consulConfig = app.Configuration.GetSection("ConsulConfig").Get<ConsulConfig>();

        if (consulConfig == null)
        {
            throw new Exception(nameof(consulConfig));
        }

        var consulClient = new ConsulClient(config =>
        {
            config.Address = new Uri($"http://{consulConfig.ConsulHost}:{consulConfig.ConsulPort}");
        });

        var registration = new AgentServiceRegistration
        {
            ID = consulConfig.ServiceId,
            Name = consulConfig.ServiceName,
            Address = GetLocalIPAddress(),
            Port = 5000,
            Check = new AgentServiceCheck
            {
                HTTP = $"http://{GetLocalIPAddress()}:5000/api/healthcheck",
                Interval = TimeSpan.FromSeconds(10),
                Timeout = TimeSpan.FromSeconds(5),
                DeregisterCriticalServiceAfter = TimeSpan.FromSeconds(30)
            }
        };
    }

    private static string GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }
        throw new Exception("Local IP Address Not Found!");
    }
}