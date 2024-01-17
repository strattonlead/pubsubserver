using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace PubSubServer.Client
{
    public interface IRedisRmiClient<T>
    {
        Task InvokeAsync(Expression<Action<T>> expression, CancellationToken cancellationToken = default);

        Task InvokeAsync(Expression<Action<T>> expression, TimeSpan? timeout, CancellationToken cancellationToken = default);

        Task<TResult> InvokeAsync<TResult>(Expression<Func<T, TResult>> expression, CancellationToken cancellationToken = default);

        Task<TResult> InvokeAsync<TResult>(Expression<Func<T, TResult>> expression, TimeSpan? timeout, CancellationToken cancellationToken = default);
    }
}
