using System;
using System.Threading;
using System.Threading.Tasks;
using Consul;
using Microsoft.Extensions.Hosting;

namespace Infrastructure.ServiceDiscovery
{
    public class ServiceDiscoveryHostedService(IConsulClient client, ServiceConfig config) : IHostedService
    {
        private readonly IConsulClient _client = client;
        private readonly ServiceConfig _config = config;
        private string _registrationId;

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _registrationId = $"{_config.Name}-{_config.Id}";

            var registration = new AgentServiceRegistration
            {
                ID = _registrationId,
                Name = _config.Name,
                Address = _config.Address.Host,
                Port = _config.Port,
                Check = new AgentServiceCheck
                {
//                    HTTP = _config.HealthCheckEndPoint,
                    HTTP = $"http://{_config.Address}:{_config.Port}/api/values/{_config.HealthCheckEndPoint}",
                    Interval = TimeSpan.FromSeconds(15),
                    Timeout = TimeSpan.FromSeconds(5),
                    DeregisterCriticalServiceAfter = TimeSpan.FromSeconds(5)
                }
            };

            await _client.Agent.ServiceDeregister(registration.ID, cancellationToken).ConfigureAwait(false);
            await _client.Agent.ServiceRegister(registration, cancellationToken).ConfigureAwait(false);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _client.Agent.ServiceDeregister(_registrationId, cancellationToken).ConfigureAwait(false);
        }
    }
}