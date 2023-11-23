using System.Threading.Tasks;

namespace PubSubServer.Redis
{
    public interface IStackService
    {
        Task<long> PushAsync<T>(string stackName, T value);
        Task<long> PushAsync(string stackName, string value);

        Task<string> PopAsync(string stackName);
        Task<T> PopAsync<T>(string queustackNameeName);
    }
}
