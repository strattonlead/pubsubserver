using Newtonsoft.Json;

namespace PubSubServer.Redis
{
    public class PubSubMessage
    {
        [JsonProperty(PropertyName = "topic")]
        public string Topic { get; set; }

        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }
    }
}
