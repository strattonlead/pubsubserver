using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace PubSubServer.Redis
{
    public class PubSubOptionsBuilder
    {
        internal PubSubOptions Options = new PubSubOptions();

        public PubSubOptionsBuilder UseIsActive(bool isActive)
        {
            Options.IsActive = isActive;
            return this;
        }

        public PubSubOptionsBuilder UseConnectionString(string connectionString)
        {
            Options.ConnectionString = connectionString;
            return this;
        }

        public PubSubOptionsBuilder UseDefaultChannel(string channel)
        {
            Options.DefaultChannel = channel;
            return this;
        }
    }

    public static class RedisPubSubServiceExtensions
    {
        public static void AddRedisServices(this IServiceCollection services, Action<PubSubOptionsBuilder> builder)
        {
            services.AddRedisPubSubService(builder);
            services.AddRedisStackService(builder);
            services.AddRedisQueueService(builder);
        }

        public static void AddRedisPubSubService(this IServiceCollection services, Action<PubSubOptionsBuilder> builder)
        {
            var optionsBuilder = new PubSubOptionsBuilder();
            builder(optionsBuilder);
            services.TryAddSingleton(optionsBuilder.Options);
            services.TryAddSingleton<IPubSubService, PubSubService>();
        }

        public static void AddRedisStackService(this IServiceCollection services, Action<PubSubOptionsBuilder> builder)
        {
            var optionsBuilder = new PubSubOptionsBuilder();
            builder(optionsBuilder);
            services.TryAddSingleton(optionsBuilder.Options);
            services.TryAddSingleton<IStackService, StackService>();
        }

        public static void AddRedisQueueService(this IServiceCollection services, Action<PubSubOptionsBuilder> builder)
        {
            var optionsBuilder = new PubSubOptionsBuilder();
            builder(optionsBuilder);
            services.TryAddSingleton(optionsBuilder.Options);
            services.TryAddSingleton<IQueueService, QueueService>();
        }

        public static void AddRedisPubSubService(this IServiceCollection services)
        {
            services.TryAddSingleton(new PubSubOptions());
            services.TryAddSingleton<IPubSubService, PubSubService>();
        }

        public static void AddRedisStackService(this IServiceCollection services)
        {
            services.TryAddSingleton(new PubSubOptions());
            services.TryAddSingleton<IStackService, StackService>();
        }

        public static void AddRedisQueueService(this IServiceCollection services)
        {
            services.TryAddSingleton(new PubSubOptions());
            services.TryAddSingleton<IQueueService, QueueService>();
        }
    }
}