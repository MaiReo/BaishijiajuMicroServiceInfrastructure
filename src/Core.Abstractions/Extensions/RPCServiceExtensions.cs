using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Core.RemoteCall
{
    public static class RPCServiceExtensions
    {
        #region Synchronous 

        public static RPCHttpResult<TResult> Map<TSource, TResult>(this IRPCService @this, Func<TSource, TResult> selector, string serviceName,
            string relativePath,
            object data = default,
            IDictionary<string, string> headers = default,
            HttpMethod httpMethod = default)
            where TSource : class, new()
        {
            if (selector == default)
            {
                throw new ArgumentNullException(nameof(selector));
            }

            var httpResult = @this.CallHttpServiceAsync<TSource>(serviceName, relativePath, httpMethod, data, default, headers).GetAwaiter().GetResult();
            var mappedHttpResult = new RPCHttpResult<TResult>(httpResult.StatusCode, httpResult.Exception);
            if (httpResult.Exception != default)
            {
                return mappedHttpResult;
            }
            if (httpResult.Result == default)
            {
                return mappedHttpResult;
            }
            mappedHttpResult.Result = selector(httpResult.Result);
            return mappedHttpResult;
        }

        #endregion Synchronous 

        #region Asynchronous 
        public static async ValueTask<RPCHttpResult<TResult>> MapAsync<TSource, TResult>(this IRPCService @this, Func<TSource, ValueTask<TResult>> selector, string serviceName,
            string relativePath,
            object data = default,
            IDictionary<string, string> headers = default,
            HttpMethod httpMethod = default, CancellationToken cancellationToken = default)
            where TSource : class, new()
        {
            if (selector == default)
            {
                throw new ArgumentNullException(nameof(selector));
            }
            return await MapAsync(@this, async (TSource source, CancellationToken _) => await selector(source), serviceName, relativePath, data, headers, httpMethod, cancellationToken);
        }

        public static async Task<RPCHttpResult<TResult>> MapAsync<TSource, TResult>(this IRPCService @this, Func<TSource, CancellationToken, ValueTask<TResult>> selector, string serviceName,
            string relativePath,
            object data = default,
            IDictionary<string, string> headers = default,
            HttpMethod httpMethod = default, CancellationToken cancellationToken = default)
            where TSource : class, new()
        {
            if (selector == default)
            {
                throw new ArgumentNullException(nameof(selector));
            }

            var httpResult = await @this.CallHttpServiceAsync<TSource>(serviceName, relativePath, httpMethod, data, default, headers, cancellationToken);
            var mappedHttpResult = new RPCHttpResult<TResult>(httpResult.StatusCode, httpResult.Exception);
            if (httpResult.Exception != default)
            {
                return mappedHttpResult;
            }
            if (httpResult.Result == default)
            {
                return mappedHttpResult;
            }
            mappedHttpResult.Result = await selector(httpResult.Result, cancellationToken);
            return mappedHttpResult;
        }
        #endregion Asynchronous 
    }
}
