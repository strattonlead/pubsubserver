using System;
using System.Threading;
using System.Threading.Tasks;

namespace PubSubServer.Client
{
    public interface IStackClient
    {
        Task<long> PushAsync(string queueName, string value, CancellationToken cancellationToken = default);
        Task<long> PushAsync<T>(string queueName, T value, CancellationToken cancellationToken = default);
        Task<string> PopAsync(string queueName, CancellationToken cancellationToken = default);
        Task<T> PopAsync<T>(string queueName, CancellationToken cancellationToken = default);
        Task SubscribeAsync(string queueName, Action<long> callback, CancellationToken cancellationToken = default);
        Task SubscribeAsync(string queueName, Func<long, Task> callback, CancellationToken cancellationToken = default);

    }
}
