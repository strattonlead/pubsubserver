﻿using Microsoft.Extensions.DependencyInjection;
using PubSubServer.Redis;
using System;
using System.Threading.Tasks;

namespace PubSubServer.Client
{
    public class PubSubClient
    {
        #region Properties

        private readonly RedisPubSubService _redisPubSub;
        private readonly string _channel;
        #endregion

        #region Constructor

        public PubSubClient(IServiceProvider serviceProvider)
        {
            _redisPubSub = serviceProvider.GetRequiredService<RedisPubSubService>();
            _channel = _redisPubSub.DefaultChannel;
        }

        #endregion

        #region Actions

        public async Task Publish(string topic, string message)
        {
            await _redisPubSub.PublishAsync(_channel, new PubSubMessage() { Topic = topic, Message = message });
        }

        #endregion
    }


    public static class PubSubClientDI
    {
        public static void AddPubSubClient(this IServiceCollection services)
        {
            services.AddSingleton<PubSubClient>();
        }
    }
}