using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using PubSubServer.Redis;
using System;
using System.Threading.Tasks;

namespace PubSubServer.Client
{
    public class PubSubClient : IPubSubClient
    {
        #region Properties

        private readonly IPubSubService _redisPubSub;
        private readonly string _channel;
        #endregion

        #region Constructor

        public PubSubClient(IServiceProvider serviceProvider)
        {
            _redisPubSub = serviceProvider.GetRequiredService<IPubSubService>();
            _channel = _redisPubSub.DefaultChannel;
        }

        #endregion

        #region Actions

        public async Task PublishAsync(string topic, object message)
        {
            await PublishAsync(topic, JsonConvert.SerializeObject(message));
        }

        public async Task PublishAsync(string topic, string message)
        {
            await _redisPubSub.PublishAsync(_channel, new PubSubMessage() { Topic = topic, Message = message });
        }

        #endregion
    }


    public static class PubSubClientDI
    {
        public static void AddPubSubClient(this IServiceCollection services)
        {
            services.AddSingleton<IPubSubClient, PubSubClient>();
        }
    }
}