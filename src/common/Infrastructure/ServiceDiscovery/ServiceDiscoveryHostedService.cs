using Consul;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.ServiceDiscovery;

public class ServiceDiscoveryHostedService(IConsulClient client, IOptions<ServiceConfigOptions> config, ILogger<ServiceDiscoveryHostedService> logger, IServer server, IApplicationLifetime lifetime) : IHostedService
{
    private readonly IApplicationLifetime _applicationLifetime = lifetime;
    private CancellationTokenSource _cts;
    private readonly IOptions<ServiceConfigOptions> _consulConfig = config;
    private readonly ILogger<ServiceDiscoveryHostedService> _logger = logger;
    private readonly IServer _server = server;

    private readonly IConsulClient _client = client;
    private string _registrationId;

    public static string[] Tags { get; set; }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // Create a linked token so we can trigger cancellation outside of this token's cancellation
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        _logger.LogInformation("before application has started");

        _applicationLifetime.ApplicationStarted.Register(
              async () =>
              {
                  Console.WriteLine($"Addresses after application has started: {GetAddresses()}");
                  Uri uri;
                  if (_consulConfig.Value.UseServiceSettings)
                  {
                      uri = new Uri($"{_consulConfig.Value.ServiceHost}:{_consulConfig.Value.ServicePort}");
                  }
                  else
                  {
                      var features = _server.Features;
                      var addresses = features.Get<IServerAddressesFeature>();
                      var address = addresses.Addresses.First();
                      uri = new Uri(address);
                  }




                  _registrationId = $"{_consulConfig.Value.ServiceId}"; //-{uri.Port}

                  var registration = new AgentServiceRegistration
                  {
                      ID = _registrationId,
                      Name = _consulConfig.Value.ServiceName,
                      Address = _consulConfig.Value.ServiceHost,
                      Port = _consulConfig.Value.ServicePort,
                      Tags = Tags,
                      Check = new AgentServiceCheck()
                      {
                          HTTP = $"{uri.Scheme}://{uri.Host}:{uri.Port}/{_consulConfig.Value.HealthCheckUrlSegment}",
                          Interval = TimeSpan.FromSeconds(_consulConfig.Value.HealthCheckIntervalSeconds),
                          Timeout = TimeSpan.FromSeconds(_consulConfig.Value.HealthCheckTimeoutSeconds),
                          DeregisterCriticalServiceAfter = TimeSpan.FromSeconds(30),
                      }
                  };

                  _logger.LogInformation($"Registering service with Consul: {registration.Name}");
                  await _client.Agent.ServiceDeregister(registration.ID, _cts.Token);
                  await _client.Agent.ServiceRegister(registration, _cts.Token);
              }
              );

        _logger.LogInformation("after application has started");

    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _cts.Cancel();
        _logger.LogInformation($"Deregistering service from Consul: {_registrationId}");
        try
        {
            await _client.Agent.ServiceDeregister(_registrationId, cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Deregistration failed");
        }
    }

    private string GetAddresses()
    {
        var addresses = _server.Features.Get<IServerAddressesFeature>().Addresses;
        return string.Join(", ", addresses);
    }

    private Task WaitForApplicationStartedAsync()
    {
        var completionSource = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        _applicationLifetime.ApplicationStarted.Register(() => completionSource.TrySetResult());
        return completionSource.Task;
    }
}
