using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace PubSubServer.Client
{
    public static class DI
    {
        public static void AddPubSubClient(this IServiceCollection services)
        {
            services.TryAddSingleton<IPubSubClient, PubSubClient>();
        }

        public static void AddQueueClient(this IServiceCollection services)
        {
            services.TryAddSingleton<IQueueClient, QueueClient>();
        }

        public static void AddStackClient(this IServiceCollection services)
        {
            services.TryAddSingleton<IStackClient, StackClient>();
        }
    }
}
