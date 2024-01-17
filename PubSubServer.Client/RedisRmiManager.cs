using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PubSubServer.Client
{
    public class RedisRmiManager
    {
        #region Properties

        private readonly IPubSubClient _client;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private List<Type> _registeredHandlers = new List<Type>();

        #endregion

        #region Constructor

        public RedisRmiManager(IPubSubClient pubSubClient, IServiceScopeFactory serviceScopeFactory)
        {
            _client = pubSubClient;
            _serviceScopeFactory = serviceScopeFactory;
        }

        #endregion

        #region RedisRmiHandler

        public async Task RegisterSingletonHandlerAsync<T>(T handler, CancellationToken cancellationToken = default)
        {
            var type = typeof(T);
            if (_registeredHandlers.Contains(type))
            {
                return;
            }
            _registeredHandlers.Add(type);

            var channelName = GetRmiChannelName<T>();
            await _client.SubscribeAsync<MethodCallParams>(channelName, async methodCallParams =>
            {
                try
                {
                    await _handleMethodCallAsync(handler, methodCallParams, cancellationToken);
                }
                catch { }
            }, cancellationToken);
        }

        public async Task RegisterScopedHandlerAsync<T>(CancellationToken cancellationToken = default)
        {
            var type = typeof(T);
            if (_registeredHandlers.Contains(type))
            {
                return;
            }
            _registeredHandlers.Add(type);

            var channelName = GetRmiChannelName<T>();
            await _client.SubscribeAsync<MethodCallParams>(channelName, async methodCallParams =>
             {
                 try
                 {
                     using (var scope = _serviceScopeFactory.CreateScope())
                     {
                         var handler = scope.ServiceProvider.GetRequiredService<T>();
                         await _handleMethodCallAsync(handler, methodCallParams, cancellationToken);
                     }
                 }
                 catch { }
             }, cancellationToken);
        }

        private async Task _handleMethodCallAsync<T>(T handler, MethodCallParams methodCallParams, CancellationToken cancellationToken = default)
        {
            var method = typeof(T).GetMethods().FirstOrDefault(x => x.Name == methodCallParams.MethodName);
            object[] parameters = null;

            if (methodCallParams.Params != null)
            {
                var parameterList = new List<object>();
                var count = Math.Min(methodCallParams.Params.Length, method.GetParameters().Length);
                for (var i = 0; i < count; i++)
                {
                    var expectedParameterInfo = method.GetParameters()[i];
                    var providedParameterValue = methodCallParams.Params[i];

                    if (providedParameterValue != null && providedParameterValue.GetType() == expectedParameterInfo.ParameterType)
                    {
                        parameterList.Add(providedParameterValue);
                    }
                    else
                    {
                        parameterList.Add(null);
                    }
                }
                parameters = parameterList.ToArray();
            }

            var result = method.Invoke(handler, parameters);

            if (methodCallParams.CallbackId != null)
            {
                await _client.PublishAsync(methodCallParams.CallbackId, result, cancellationToken);
            }
        }

        #endregion

        #region Helpers

        public string GetRmiChannelName<T>() => $"CallFrom_RedisRmiClient<{typeof(T).AssemblyQualifiedName}>";

        #endregion
    }
}
