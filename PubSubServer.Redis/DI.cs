using Microsoft.Extensions.DependencyInjection;
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
        public static void AddRedisPubSubService(this IServiceCollection services, Action<PubSubOptionsBuilder> builder)
        {
            var optionsBuilder = new PubSubOptionsBuilder();
            builder(optionsBuilder);
            services.AddSingleton(optionsBuilder.Options);
            services.AddSingleton<IPubSubService, PubSubService>();
        }

        public static void AddRedisPubSubService(this IServiceCollection services)
        {
            services.AddSingleton(new PubSubOptions());
            services.AddSingleton<IPubSubService, PubSubService>();
        }
    }
}