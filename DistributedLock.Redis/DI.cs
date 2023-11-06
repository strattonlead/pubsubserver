using Microsoft.Extensions.DependencyInjection;
using System;

namespace DistributedLock.Redis
{
    public class DistributedLockOptionsBuilder
    {
        internal DistributedLockOptions Options = new DistributedLockOptions();

        public DistributedLockOptionsBuilder UseConnectionString(string connectionString)
        {
            Options.ConnectionString = connectionString;
            return this;
        }
    }

    public static class DistributedLockServiceExtensions
    {
        public static void AddDistributedLockService(this IServiceCollection services, Action<DistributedLockOptionsBuilder> builder)
        {
            var optionsBuilder = new DistributedLockOptionsBuilder();
            builder(optionsBuilder);
            services.AddSingleton(optionsBuilder.Options);
            services.AddSingleton<IDistributedLockService, DistributedLockService>();
        }
    }
}
