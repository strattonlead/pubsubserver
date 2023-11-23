using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using PubSubServer.Redis;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PubSubServer.Client
{
    public class QueueClient : IQueueClient
    {
        #region Properties

        private readonly IPubSubService _redisPubSub;
        private readonly IQueueService _redisQueue;
        #endregion

        #region Constructor

        public QueueClient(IServiceProvider serviceProvider)
        {
            _redisPubSub = serviceProvider.GetRequiredService<IPubSubService>();
            _redisQueue = serviceProvider.GetRequiredService<IQueueService>();
        }

        #endregion

        #region 

        public async Task<long> PushAsync(string queueName, string value, CancellationToken cancellationToken = default)
        {
            var result = await _redisQueue.PushAsync(queueName, value);
            await _redisPubSub.PublishAsync(queueName, result, cancellationToken);
            return result;
        }

        public async Task<long> PushAsync<T>(string queueName, T value, CancellationToken cancellationToken = default)
        {
            var stringValue = JsonConvert.SerializeObject(value);
            return await PushAsync(queueName, stringValue, cancellationToken);
        }

        public async Task<string> PopAsync(string queueName, CancellationToken cancellationToken = default)
        {
            return await _redisQueue.PopAsync(queueName);
        }

        public async Task<T> PopAsync<T>(string queueName, CancellationToken cancellationToken = default)
        {
            return await _redisQueue.PopAsync<T>(queueName);
        }

        public async Task SubscribeAsync(string queueName, Action<long> callback, CancellationToken cancellationToken = default)
        {
            await _redisPubSub.SubscribeAsync(queueName, callback, cancellationToken);
        }

        public async Task SubscribeAsync(string queueName, Func<long, Task> callback, CancellationToken cancellationToken = default)
        {
            await _redisPubSub.SubscribeAsync(queueName, callback, cancellationToken);
        }

        #endregion
    }
}
