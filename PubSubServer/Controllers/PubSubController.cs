//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.SignalR;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Logging;
//using PubSubServer.Hubs;
//using System;
//using System.Threading.Tasks;

//namespace PubSubServer.Controllers
//{
//    [ApiController]
//    [Route("[controller]")]
//    public class PubSubController : ControllerBase
//    {
//        #region Properties

//        private readonly ILogger _logger;
//        private readonly IHubContext<PubSubHub, IPubSub> _hubContext;

//        #endregion

//        #region Constructor

//        public PubSubController(IServiceProvider serviceProvider)
//        {
//            _logger = serviceProvider.GetRequiredService<ILogger<PubSubController>>();
//            _hubContext = serviceProvider.GetRequiredService<IHubContext<PubSubHub, IPubSub>>();
//        }

//        #endregion

//        #region Actions

//        [HttpGet, Route("[controller]/publish")]
//        public async Task Publish(string topic, string message)
//        {
//            _logger?.LogInformation($"Publish topic -> {topic} message -> {message}");
//            await _hubContext.Clients.Group(topic).Publish(message);
//        }

//        #endregion
//    }
//}