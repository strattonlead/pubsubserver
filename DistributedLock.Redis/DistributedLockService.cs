using Microsoft.Extensions.DependencyInjection;
using RedLockNet;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedLock.Redis
{
    public interface IDistributedLockService : IDistributedLockFactory { }

    public class DistributedLockService : IDistributedLockService, IDisposable
    {
        #region Properties

        private readonly ConnectionMultiplexer _connection;
        private readonly RedLockFactory _redLockFactory;
        private readonly DistributedLockOptions _options;

        #endregion

        #region Constructor

        public DistributedLockService(IServiceProvider serviceProvider)
        {
            _options = serviceProvider.GetRequiredService<DistributedLockOptions>();
            _connection = ConnectionMultiplexer.Connect(_options.ConnectionString);
            var endPoints = new List<RedLockMultiplexer>() { _connection };
            _redLockFactory = RedLockFactory.Create(endPoints);
        }

        #endregion

        #region IDistributedLockService

        public IRedLock CreateLock(string resource, TimeSpan expiryTime)
        {
            return _redLockFactory.CreateLock(resource, expiryTime);
        }

        public IRedLock CreateLock(string resource, TimeSpan expiryTime, TimeSpan waitTime, TimeSpan retryTime, CancellationToken? cancellationToken = null)
        {
            return _redLockFactory.CreateLock(resource, expiryTime, waitTime, retryTime, cancellationToken);
        }

        public async Task<IRedLock> CreateLockAsync(string resource, TimeSpan expiryTime)
        {
            return await _redLockFactory.CreateLockAsync(resource, expiryTime);
        }

        public async Task<IRedLock> CreateLockAsync(string resource, TimeSpan expiryTime, TimeSpan waitTime, TimeSpan retryTime, CancellationToken? cancellationToken = null)
        {
            return await _redLockFactory.CreateLockAsync(resource, expiryTime, waitTime, retryTime, cancellationToken);
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            _redLockFactory.Dispose();
        }

        #endregion
    }
}