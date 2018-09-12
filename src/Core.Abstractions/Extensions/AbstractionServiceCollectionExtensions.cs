using Core.Messages.Bus;
using Core.ServiceDiscovery;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class AbstractionServiceCollectionExtensions
    {
        public static IServiceCollection AddMessageBus(this IServiceCollection services, Action<MessageBusOptions> optionsAction)
        {
            var options = new MessageBusOptions();
            optionsAction?.Invoke(options);
            services.TryAddSingleton<IMessageBusOptions>(options);
            return services;
        }
        public static IServiceCollection AddMessageBus(this IServiceCollection services, Func<IServiceProvider, MessageBusOptions> optionsAction)
        {
            services.TryAddSingleton<IMessageBusOptions>(optionsAction);
            return services;
        }

        public static IServiceCollection AddServiceDiscovery(this IServiceCollection services, Action<ServiceDiscoveryConfiguration> optionsAction)
        {
            var options = new ServiceDiscoveryConfiguration();
            optionsAction?.Invoke(options);
            services.TryAddSingleton(options);
            return services;
        }
    }
}
