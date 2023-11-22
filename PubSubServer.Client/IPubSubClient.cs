using System;
using System.Threading;
using System.Threading.Tasks;

namespace PubSubServer.Client
{
    public interface IPubSubClient
    {
        Task PublishAsync(string topic, object message, CancellationToken cancellationToken = default);
        Task PublishAsync(string topic, string message, CancellationToken cancellationToken = default);
        Task SubscribeAsync(string topic, Action callback, CancellationToken cancellationToken = default);
        Task SubscribeAsync<T>(string topic, Action<T> callback, CancellationToken cancellationToken = default);
        Task SubscribeAsync(string topic, Action<string> callback, CancellationToken cancellationToken = default);
    }
}
