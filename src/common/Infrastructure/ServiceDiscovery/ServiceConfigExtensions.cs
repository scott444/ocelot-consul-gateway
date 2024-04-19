using Consul;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace Infrastructure.ServiceDiscovery;

public static class ServiceConfigExtensions
{
    public static ServiceConfig GetServiceConfig(this IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var serviceConfig = new ServiceConfig
        {
            Id = configuration.GetValue<string>("ServiceConfig:serviceId"),
            Name = configuration.GetValue<string>("ServiceConfig:serviceName"),
            DiscoveryAddress = configuration.GetValue<Uri>("ServiceConfig:serviceDiscoveryAddress"),
            Address = configuration.GetValue<Uri>("ServiceConfig:serviceAddress"),
            Port = configuration.GetValue<int>("ServiceConfig:vPort"),
            HealthCheckEndPoint = configuration.GetValue<string>("ServiceConfig:HealthCheckEndPoint"),
        };

        return serviceConfig;
    }
}
