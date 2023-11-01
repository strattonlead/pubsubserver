using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PubSubServer.Hubs;
using PubSubServer.Redis;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PubSubServer.Services
{
    public class WorkerService : BackgroundService
    {
        #region Properties

        private readonly PubSubService _redisPubSub;
        private readonly IHostApplicationLifetime _applicationLifetime;
        private readonly ILogger _logger;
        private readonly IServiceProvider _serviceProvider;

        #endregion

        #region Constructor

        public WorkerService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _redisPubSub = serviceProvider.GetRequiredService<PubSubService>();
            _applicationLifetime = serviceProvider.GetRequiredService<IHostApplicationLifetime>();
            _logger = serviceProvider.GetRequiredService<ILogger<WorkerService>>();
        }

        #endregion

        #region BackgroundService

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var channel = _redisPubSub.DefaultChannel;
            _logger?.LogInformation($"SubscribeAsync -> {channel}");

            await _redisPubSub.SubscribeAsync<PubSubMessage>(channel, async pubSubMessage =>
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<PubSubHub, IPubSub>>();

                    _logger?.LogInformation($"Publish topic -> {pubSubMessage.Topic} message -> {pubSubMessage.Message}");
                    await hubContext.Clients.Group(pubSubMessage.Topic).Publish(pubSubMessage.Message);
                }
            }, _applicationLifetime.ApplicationStopping);
        }

        #endregion
    }
}
