using System;

namespace PubSubServer.Redis
{
    public class PubSubOptions
    {
        public string ConnectionString { get; set; } = Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING");
        public string DefaultChannel { get; set; } = Environment.GetEnvironmentVariable("REDIS_PUBSUB_CHANNEL") ?? "pubsub";
    }
}
