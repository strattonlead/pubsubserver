using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using PubSubServer.Redis;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PubSubServer.Client
{
    public class StackClient : IStackClient
    {
        #region Properties

        private readonly IPubSubService _redisPubSub;
        private readonly IStackService _redisStack;
        #endregion

        #region Constructor

        public StackClient(IServiceProvider serviceProvider)
        {
            _redisPubSub = serviceProvider.GetRequiredService<IPubSubService>();
            _redisStack = serviceProvider.GetRequiredService<IStackService>();
        }

        #endregion

        #region IStackClient

        public async Task<long> PushAsync(string stackName, string value, CancellationToken cancellationToken = default)
        {
            var result = await _redisStack.PushAsync(stackName, value);
            await _redisPubSub.PublishAsync(stackName, result, cancellationToken);
            return result;
        }

        public async Task<long> PushAsync<T>(string stackName, T value, CancellationToken cancellationToken = default)
        {
            var stringValue = JsonConvert.SerializeObject(value);
            return await PushAsync(stackName, stringValue, cancellationToken);
        }

        public async Task<string> PopAsync(string stackName, CancellationToken cancellationToken = default)
        {
            return await _redisStack.PopAsync(stackName);
        }

        public async Task<T> PopAsync<T>(string stackName, CancellationToken cancellationToken = default)
        {
            return await _redisStack.PopAsync<T>(stackName);
        }

        public async Task SubscribeAsync(string stackName, Action<long> callback, CancellationToken cancellationToken = default)
        {
            await _redisPubSub.SubscribeAsync(stackName, callback, cancellationToken);
        }

        public async Task SubscribeAsync(string stackName, Func<long, Task> callback, CancellationToken cancellationToken = default)
        {
            await _redisPubSub.SubscribeAsync(stackName, callback, cancellationToken);
        }

        #endregion
    }
}

