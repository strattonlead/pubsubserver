using System;
using System.Threading;
using System.Threading.Tasks;

namespace PubSubServer.Redis
{
    public interface IPubSubService
    {
        string DefaultChannel { get; }
        Task<long> PublishAsync(string channel, CancellationToken cancellationToken = default);
        Task<long> PublishAsync(string channel, object obj, CancellationToken cancellationToken = default);
        Task<long> PublishAsync(string channel, string message, CancellationToken cancellationToken = default);

        Task SubscribeAsync(string channel, Action callback, CancellationToken cancellationToken = default);
        Task SubscribeAsync<T>(string channel, Action<T> callback, CancellationToken cancellationToken = default);
        Task SubscribeAsync(string channel, Action<string> callback, CancellationToken cancellationToken = default);

        Task UnsubscribeAsync(string channel, CancellationToken cancellationToken = default);
        Task UnsubscribeAllAsync(CancellationToken cancellationToken = default);
    }
}