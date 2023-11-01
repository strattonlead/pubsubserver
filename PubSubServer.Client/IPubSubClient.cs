using System.Threading.Tasks;

namespace PubSubServer.Client
{
    public interface IPubSubClient
    {
        Task PublishAsync(string topic, object message);
        Task PublishAsync(string topic, string message);
    }
}
