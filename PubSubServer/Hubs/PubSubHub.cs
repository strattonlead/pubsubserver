using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;

namespace PubSubServer.Hubs
{
    public interface IPubSub
    {
        Task Publish(string message);
        Task Subscribed(string topic);
        Task Unsubscribed(string topic);
    }

    public class PubSubHub : Hub<IPubSub>
    {
        #region Properties

        private readonly IHostApplicationLifetime _applicationLifetime;

        #endregion

        #region Constructors

        public PubSubHub(IServiceProvider serviceProvider)
        {
            _applicationLifetime = serviceProvider.GetRequiredService<IHostApplicationLifetime>();
        }

        #endregion

        #region Actions

        public async Task Subscribe(string topic, bool @public)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, topic, _applicationLifetime.ApplicationStopping);
            if (@public)
            {
                await Clients.All.Subscribed(topic);
            }
            else
            {
                await Clients.Client(Context.ConnectionId).Subscribed(topic);
            }
        }

        public async Task Unsubscribe(string topic, bool @public)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, topic, _applicationLifetime.ApplicationStopping);
            if (@public)
            {
                await Clients.All.Unsubscribed(topic);
            }
            else
            {
                await Clients.Client(Context.ConnectionId).Unsubscribed(topic);
            }
        }

        #endregion
    }
}
