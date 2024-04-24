using Consul;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Options;

namespace MinimalApi.Infrastructure;

public class ConsulHostedService(IConsulClient consulClient, IOptions<ConsulConfig> consulConfig, ILogger<ConsulHostedService> logger, IServer server) : IHostedService
{
    private Task? _executingTask;
    private CancellationTokenSource? _cts;
    private readonly IConsulClient _consulClient = consulClient;
    private readonly IOptions<ConsulConfig> _consulConfig = consulConfig;
    private readonly ILogger<ConsulHostedService> _logger = logger;
    private readonly IServer _server = server;
    private string? _registrationID;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // Create a linked token so we can trigger cancellation outside of this token's cancellation
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        var features = _server.Features;
        var addresses = features.Get<IServerAddressesFeature>();
        var address = addresses.Addresses.First();

        var uri = new Uri(address);
        _registrationID = $"{_consulConfig.Value.ServiceID}-{uri.Port}";

        var registration = new AgentServiceRegistration()
        {
            ID = _registrationID,
            Name = _consulConfig.Value.ServiceName,
            Address = $"{uri.Scheme}://{uri.Host}",
            Port = uri.Port,
            Tags = ["minimal-api"],
            Check = new AgentServiceCheck()
            {
                HTTP = $"{uri.Scheme}://{uri.Host}:{uri.Port}/healthcheck",
                Timeout = TimeSpan.FromSeconds(3),
                Interval = TimeSpan.FromSeconds(10),
                DeregisterCriticalServiceAfter = TimeSpan.FromSeconds(30),
            }
        };

        _logger.LogInformation("Registering in Consul");
        await _consulClient.Agent.ServiceDeregister(registration.ID, _cts.Token);
        await _consulClient.Agent.ServiceRegister(registration, _cts.Token);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _cts.Cancel();
        _logger.LogInformation("Deregistering from Consul");
        try
        {
            await _consulClient.Agent.ServiceDeregister(_registrationID, cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Deregistration failed");
        }
    }
}
