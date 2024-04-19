using System;

namespace Infrastructure.ServiceDiscovery
{
    public class ServiceConfig
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public Uri DiscoveryAddress { get; set; }
        public Uri Address { get; set; }
        public int Port { get; set; }
        public string HealthCheckEndPoint { get; set; }
    }
}