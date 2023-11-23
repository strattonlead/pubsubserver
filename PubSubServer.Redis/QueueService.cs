using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace PubSubServer.Redis
{
    public class QueueService : IQueueService, IDisposable
    {
        #region Properties

        private readonly PubSubOptions _options;
        private readonly ConnectionMultiplexer _connection;
        private readonly IDatabase _database;
        public bool IsConnected => _connection.IsConnected;

        #endregion

        #region Constructor

        public QueueService(IServiceProvider serviceProvider)
        {
            _options = serviceProvider.GetRequiredService<PubSubOptions>();
            if (_options.IsActive)
            {
                _connection = ConnectionMultiplexer.Connect(_options.ConnectionString);
                _database = _connection.GetDatabase();
            }

        }

        #endregion

        #region QueueService

        public async Task<long> PushAsync<T>(string queueName, T value)
        {
            var redisValue = JsonConvert.SerializeObject(value);
            return await PushAsync(queueName, redisValue);
        }

        public async Task<long> PushAsync(string queueName, string value)
        {
            return await _database.ListRightPushAsync(queueName, value);
        }

        public async Task<string> PopAsync(string queueName)
        {
            return await _database.ListLeftPopAsync(queueName);
        }

        public async Task<T> PopAsync<T>(string queueName)
        {
            var redisValue = await PopAsync(queueName);
            return JsonConvert.DeserializeObject<T>(redisValue);
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
