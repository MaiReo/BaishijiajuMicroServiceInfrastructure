using Core.Messages.Bus;
using Core.ServiceDiscovery;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class AbstractionServiceCollectionExtensions
    {
        [Obsolete]
        public static IServiceCollection AddMessageBus(this IServiceCollection services, Action<MessageBusOptions> optionsAction)
        {
            var options = new MessageBusOptions();
            optionsAction?.Invoke(options);
            services.TryAddSingleton<IMessageBusOptions>(options);
            return services;
        }

        [Obsolete]
        public static IServiceCollection AddMessageBus(this IServiceCollection services, Func<IServiceProvider, IMessageBusOptions> optionsAction)
        {
            services.TryAddSingleton(optionsAction);
            return services;
        }

        public static IServiceCollection AddMessageBus(this IServiceCollection services,
            string hostName, int port,
            string virtualhostName,
            string exchangeName, string queueName,
            string userName, string password, string hostServiceName = default, bool? queuePerConsumer = default)
        {
            var options = new MessageBusOptions()
            {
                HostName = hostName,
                Port = port,
                VirtualHost = virtualhostName,
                ExchangeName = exchangeName,
                QueueName = queueName,
                UserName = userName,
                Password = password,
                HostServiceName = hostServiceName,
                UseServiceDiscovery = !string.IsNullOrWhiteSpace(hostServiceName),
                QueuePerConsumer = queuePerConsumer
            };
            services.TryAddSingleton<IMessageBusOptions>(options);
            return services;
        }

        public static IServiceCollection AddServiceDiscovery(this IServiceCollection services, Action<ServiceDiscoveryConfiguration> optionsAction)
        {
            var options = new ServiceDiscoveryConfiguration();
            optionsAction?.Invoke(options);
            services.TryAddSingleton(options);
            return services;
        }

        public static IServiceCollection AddServiceDiscovery(this IServiceCollection services, string address, string serviceName)
        {
            var options = new ServiceDiscoveryConfiguration() { Address = address, ServiceName = serviceName };
            services.TryAddSingleton(options);
            return services;
        }
    }
}
