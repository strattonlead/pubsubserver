using System;

namespace DistributedLock.Redis
{
    public class DistributedLockOptions
    {
        public string ConnectionString { get; set; } = Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING");
    }
}
