using Consul;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Options;
using MinimalApi.Infrastructure;
using System.Net;
using System.Net.Sockets;

namespace MinimalApi.Extensions;

public static class ConsulExtensions
{
    public static void RegisterWithConsul(this IApplicationBuilder app, IHostApplicationLifetime lifetime)
    {
        // Retrieve Consul client from DI
        var consulClient = app.ApplicationServices.GetRequiredService<IConsulClient>();

        var consulConfig = app.ApplicationServices.GetRequiredService<IOptions<ConsulConfig>>();

        // Setup logger
        var loggingFactory = app.ApplicationServices.GetRequiredService<ILoggerFactory>();
        var logger = loggingFactory.CreateLogger<IApplicationBuilder>();

        // Get server IP address
        var features = app.Properties["server.Features"] as FeatureCollection;
        var addresses = features?.Get<IServerAddressesFeature>();
        //var address = addresses?.Addresses.First();

        // Register service with consul
        //var uri = new Uri(address);

        var uri = new Uri("http://192.168.243.23:32769");

        var httpCheck = new AgentServiceCheck
        {
            HTTP = $"{uri.Scheme}://{uri.Host}:{uri.Port}/healthcheck",
            Interval = TimeSpan.FromSeconds(10),
            Timeout = TimeSpan.FromSeconds(5),
            //DeregisterCriticalServiceAfter = TimeSpan.FromSeconds(30)
        };

        var registration = new AgentServiceRegistration
        {
            ID = $"{consulConfig.Value.ServiceID}-123",
            Name = consulConfig.Value.ServiceName,
            Address = $"{uri.Scheme}://{uri.Host}",
            Port = uri.Port,
            Tags = ["minimal"],
            Checks = [httpCheck]
        };

        logger.LogInformation("Registering with Consul");
        consulClient.Agent.ServiceDeregister(registration.ID).Wait();
        consulClient.Agent.ServiceRegister(registration).Wait();

        lifetime.ApplicationStopping.Register(() =>
        {
            logger.LogInformation("Deregistering from Consul");
            consulClient.Agent.ServiceDeregister(registration.ID).Wait();
        });

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

    private static int FreeTcpPort()
    {
        var l = new TcpListener(IPAddress.Loopback, 0);
        l.Start();
        var port = ((IPEndPoint)l.LocalEndpoint).Port;
        l.Stop();
        return port;
    }
}