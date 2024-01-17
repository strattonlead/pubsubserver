using System;

namespace PubSubServer.Redis
{
    public class PubSubOptions
    {
        public bool IsActive { get; set; } = true;
        public string ConnectionString { get; set; } = Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING");
        public string DefaultChannel { get; set; } = string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("REDIS_PUBSUB_CHANNEL")) ? "pubsub" : Environment.GetEnvironmentVariable("REDIS_PUBSUB_CHANNEL");
    }
}
