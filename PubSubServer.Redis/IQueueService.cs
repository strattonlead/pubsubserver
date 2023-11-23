using System.Threading.Tasks;

namespace PubSubServer.Redis
{
    public interface IQueueService
    {
        Task<long> PushAsync<T>(string queueName, T value);
        Task<long> PushAsync(string queueName, string value);

        Task<string> PopAsync(string queueName);
        Task<T> PopAsync<T>(string queueName);
    }
}
