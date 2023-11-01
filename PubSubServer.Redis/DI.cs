using Microsoft.Extensions.DependencyInjection;

namespace PubSubServer.Redis
{
    public static class RedisPubSubServiceExtensions
    {
        public static void AddRedisPubSubService(this IServiceCollection services)
        {
            services.AddSingleton<IPubSubService, PubSubService>();
        }
    }
}