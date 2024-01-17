using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace PubSubServer.Client
{
    public class RedisRmiClient<T> : IRedisRmiClient<T>
    {
        #region Properties

        private readonly IPubSubClient _client;
        private readonly ILogger _logger;
        public string RmiChannelName => $"CallFrom_RedisRmiClient<{typeof(T).AssemblyQualifiedName}>";

        #endregion

        #region Constructor

        public RedisRmiClient(IPubSubClient pubSubClient, ILogger<RedisRmiClient<T>> logger)
        {
            _client = pubSubClient;
            _logger = logger;
        }

        #endregion

        #region RedisRmiClient<T>

        public async Task InvokeAsync(Expression<Action<T>> expression, CancellationToken cancellationToken = default)
        {
            await InvokeAsync(expression, null, cancellationToken);
        }

        public async Task InvokeAsync(Expression<Action<T>> expression, TimeSpan? timeout, CancellationToken cancellationToken = default)
        {
            var resetEvent = new ManualResetEvent(false);
            var id = Guid.NewGuid().ToString();
            await _client.SubscribeAsync(id, () =>
            {
                resetEvent.Set();
            }, cancellationToken);

            var methodCallParams = _getMethodCallParams(expression, id);
            _logger.LogInformation($"Publish on channel {RmiChannelName} with callback {methodCallParams.CallbackId}");
            await _client.PublishAsync(RmiChannelName, methodCallParams, cancellationToken);

            if (timeout.HasValue)
            {
                resetEvent.WaitOne(timeout.Value);
            }
            else
            {
                resetEvent.WaitOne();
            }
        }

        public async Task<TResult> InvokeAsync<TResult>(Expression<Func<T, TResult>> expression, CancellationToken cancellationToken = default)
        {
            return await InvokeAsync(expression, null, cancellationToken);
        }

        public async Task<TResult> InvokeAsync<TResult>(Expression<Func<T, TResult>> expression, TimeSpan? timeout, CancellationToken cancellationToken = default)
        {
            var resetEvent = new ManualResetEvent(false);
            var id = Guid.NewGuid().ToString();
            TResult result = default;
            await _client.SubscribeAsync<TResult>(id, response =>
            {
                result = response;
                resetEvent.Set();
            }, cancellationToken);

            var methodCallParams = _getMethodCallParams(expression, id);
            _logger.LogInformation($"Publish on channel {RmiChannelName} with callback {methodCallParams.CallbackId}");
            await _client.PublishAsync(RmiChannelName, methodCallParams, cancellationToken);

            if (timeout.HasValue)
            {
                resetEvent.WaitOne(timeout.Value);
            }
            else
            {
                resetEvent.WaitOne();
            }

            return result;
        }

        #endregion

        #region Helpers

        private string _getMethodName(Expression<Action<T>> expression)
        {
            var body = expression.Body as MethodCallExpression;
            if (body == null)
            {
                throw new ArgumentException("Body must be of type MethodCallExpression. e.g. x => x.DoSomething()");
            }
            return $"{typeof(T).AssemblyQualifiedName}.{body.Method.Name}";
        }

        private string _getMethodName<TResult>(Expression<Func<T, TResult>> expression)
        {
            var body = expression.Body as MethodCallExpression;
            if (body == null)
            {
                throw new ArgumentException("Body must be of type MethodCallExpression. e.g. x => x.DoSomething()");
            }
            return $"{typeof(T).AssemblyQualifiedName}.{body.Method.Name}";
        }

        private object[] _getMethodParameters<TResult>(Expression<Func<T, TResult>> expression)
        {
            var body = expression.Body as MethodCallExpression;
            if (body == null)
            {
                throw new ArgumentException("Body must be of type MethodCallExpression. e.g. x => x.DoSomething()");
            }

            return body.Arguments.Select(x => _getParameter(x)).ToArray();
        }

        private object _getParameter(Expression expression)
        {
            MemberExpression memberExpression;
            if (expression is MemberExpression)
            {
                memberExpression = (MemberExpression)expression;
            }
            else if (expression is UnaryExpression)
            {
                memberExpression = (MemberExpression)((UnaryExpression)expression).Operand;
            }
            else
            {
                throw new NotSupportedException(expression.ToString());
            }

            return _getValue(memberExpression);
        }

        private object _getValue(MemberExpression exp)
        {
            if (exp.Expression is ConstantExpression)
            {
                return (((ConstantExpression)exp.Expression).Value)
                        .GetType()
                        .GetField(exp.Member.Name)
                        .GetValue(((ConstantExpression)exp.Expression).Value);
            }
            else if (exp.Expression is MemberExpression)
            {
                return _getValue((MemberExpression)exp.Expression);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private MethodCallParams _getMethodCallParams(Expression<Action<T>> expression, string callbackId = null)
        => new MethodCallParams()
        {
            MethodName = _getMethodName(expression),
            CallbackId = callbackId
        };

        private MethodCallParams _getMethodCallParams<TResult>(Expression<Func<T, TResult>> expression, string callbackId = null)
        => new MethodCallParams()
        {
            MethodName = _getMethodName(expression),
            CallbackId = callbackId,
            Params = _getMethodParameters(expression)
        };

        #endregion
    }

    public class MethodCallParams
    {
        public string CallbackId { get; set; }
        public string MethodName { get; set; }
        public object[] Params { get; set; }
    }

    public interface ITest
    {
        string Add(int a, long b);
    }

    public static class RedisRmiClientHelper
    {

    }
}
