using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PubSubServer.Redis
{
    public class PubSubService : IPubSubService, IDisposable
    {
        #region Properties

        private readonly PubSubOptions _options;
        private readonly ConnectionMultiplexer _connection;
        private readonly ISubscriber _pubSub;
        public bool IsConnected => _connection.IsConnected;

        #endregion

        #region Constructor

        public PubSubService(IServiceProvider serviceProvider)
        {
            _options = serviceProvider.GetRequiredService<PubSubOptions>();
            if (!_options.IsActive)
            {
                _connection = ConnectionMultiplexer.Connect(_options.ConnectionString);
                _pubSub = _connection.GetSubscriber();
            }

        }

        #endregion

        #region IPubSubService

        public string DefaultChannel => _options.DefaultChannel;

        public async Task<long> PublishAsync(string channel, CancellationToken cancellationToken = default)
        {
            return await PublishAsync(channel, "", cancellationToken);
        }

        public async Task<long> PublishAsync(string channel, object obj, CancellationToken cancellationToken = default)
        {
            var message = JsonConvert.SerializeObject(obj);
            return await PublishAsync(channel, message, cancellationToken);
        }

        public async Task<long> PublishAsync(string channel, string message, CancellationToken cancellationToken = default)
        {
            if (_connection == null)
            {
                return 0;
            }

            while (!_connection.IsConnected)
            {
                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }

            if (cancellationToken.IsCancellationRequested)
            {
                return 0;
            }

            return await _pubSub.PublishAsync(channel, message, CommandFlags.FireAndForget);
        }

        public async Task SubscribeAsync(string channel, Action callback, CancellationToken cancellationToken = default)
        {
            await SubscribeAsync(channel, x => { callback?.Invoke(); }, cancellationToken);
        }

        public async Task SubscribeAsync<T>(string channel, Action<T> callback, CancellationToken cancellationToken = default)
        {
            if (_connection == null)
            {
                return;
            }

            while (!_connection.IsConnected)
            {
                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            await _pubSub.SubscribeAsync(channel, (c, value) =>
            {
                var stringValue = $"{value}";
                var result = JsonConvert.DeserializeObject<T>(stringValue);
                callback?.Invoke(result);
            }, CommandFlags.None);
        }

        public async Task SubscribeAsync(string channel, Action<string> callback, CancellationToken cancellationToken = default)
        {
            if (_connection == null)
            {
                return;
            }

            while (!_connection.IsConnected)
            {
                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            await _pubSub.SubscribeAsync(channel, (c, value) =>
            {
                var stringValue = $"{value}";
                callback?.Invoke($"{value}");
            }, CommandFlags.None);
        }

        public async Task UnsubscribeAsync(string channel, CancellationToken cancellationToken = default)
        {
            if (_connection == null)
            {
                return;
            }

            while (!_connection.IsConnected)
            {
                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            await _pubSub.UnsubscribeAsync(channel);
        }

        public async Task UnsubscribeAllAsync(CancellationToken cancellationToken = default)
        {
            if (_connection == null)
            {
                return;
            }

            while (!_connection.IsConnected)
            {
                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            await _pubSub.UnsubscribeAllAsync(CommandFlags.None);
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            _connection?.Close();
        }

        #endregion
    }
}