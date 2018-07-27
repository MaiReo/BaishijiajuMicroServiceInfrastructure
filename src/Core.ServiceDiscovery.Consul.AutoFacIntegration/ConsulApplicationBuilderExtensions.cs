using Core.Messages;
using Core.ServiceDiscovery;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Microsoft.AspNetCore.Builder
{
    public static class ConsulApplicationBuilderExtensions
    {
        private static IServiceDiscoveryHelper _serviceDiscoveryHelper;
        /// <summary>
        /// Use RabbitMQ.
        /// Pass IApplicationLifetime.ApplicationStopping to this method.
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseConsul(this IApplicationBuilder app,
            CancellationToken applicationStopping)
        {
            _serviceDiscoveryHelper = _serviceDiscoveryHelper ?? app.ApplicationServices.GetRequiredService<IServiceDiscoveryHelper>();
            _serviceDiscoveryHelper.RegisterAsync().GetAwaiter().GetResult();
            applicationStopping.Register(() => 
            {
                try
                {
                    _serviceDiscoveryHelper.DeregisterAsync().GetAwaiter().GetResult();
                }
                catch
                {
                    // No action.
                }
            });
            return app;
        }
    }
}
