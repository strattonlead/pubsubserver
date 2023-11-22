using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using PubSubServer.Redis;
using System;
using System.Threading;
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

        public async Task PublishAsync(string topic, object message, CancellationToken cancellationToken = default)
        {
            await PublishAsync(topic, JsonConvert.SerializeObject(message), cancellationToken);
        }

        public async Task PublishAsync(string topic, string message, CancellationToken cancellationToken = default)
        {
            await _redisPubSub.PublishAsync(_channel, new PubSubMessage() { Topic = topic, Message = message }, cancellationToken);
        }

        public async Task SubscribeAsync(string topic, Action callback, CancellationToken cancellationToken = default)
        {
            await _redisPubSub.SubscribeAsync(_channel, (PubSubMessage message) =>
            {
                if (message.Topic == topic)
                {
                    callback?.Invoke();
                }
            }, cancellationToken);
        }

        public async Task SubscribeAsync<T>(string topic, Action<T> callback, CancellationToken cancellationToken = default)
        {
            await _redisPubSub.SubscribeAsync(_channel, (PubSubMessage message) =>
            {
                if (message.Topic == topic)
                {
                    var payload = JsonConvert.DeserializeObject<T>(message.Message);
                    callback?.Invoke(payload);
                }
            }, cancellationToken);
        }

        public async Task SubscribeAsync(string topic, Action<string> callback, CancellationToken cancellationToken = default)
        {
            await _redisPubSub.SubscribeAsync(_channel, (PubSubMessage message) =>
            {
                if (message.Topic == topic)
                {
                    callback?.Invoke(message.Message);
                }
            }, cancellationToken);
        }

        public async Task SubscribeAsync(string topic, Func<Task> callback, CancellationToken cancellationToken = default)
        {
            await _redisPubSub.SubscribeAsync(_channel, async (PubSubMessage message) =>
            {
                if (message.Topic == topic)
                {
                    await callback?.Invoke();
                }
            }, cancellationToken);
        }

        public async Task SubscribeAsync<T>(string topic, Func<T, Task> callback, CancellationToken cancellationToken = default)
        {
            await _redisPubSub.SubscribeAsync(_channel, async (PubSubMessage message) =>
            {
                if (message.Topic == topic)
                {
                    var payload = JsonConvert.DeserializeObject<T>(message.Message);
                    await callback?.Invoke(payload);
                }
            }, cancellationToken);
        }

        public async Task SubscribeAsync(string topic, Func<string, Task> callback, CancellationToken cancellationToken = default)
        {
            await _redisPubSub.SubscribeAsync(_channel, async (PubSubMessage message) =>
            {
                if (message.Topic == topic)
                {
                    await callback?.Invoke(message.Message);
                }
            }, cancellationToken);
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