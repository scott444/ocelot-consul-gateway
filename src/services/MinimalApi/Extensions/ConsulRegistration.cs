using Consul;
using Microsoft.Extensions.Options;
using System.Net;


namespace MinimalApi.Extensions;

public static class ConsulRegistration
{
    public static void RegisterService(IApplicationBuilder app)
    {
        var consulConfig = app.ApplicationServices.GetService<IOptions<ConsulConfig>>()?.Value;

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
