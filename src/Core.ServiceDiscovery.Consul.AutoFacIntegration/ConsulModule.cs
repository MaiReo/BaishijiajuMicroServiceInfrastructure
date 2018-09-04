using Autofac;
using Microsoft.Extensions.DependencyInjection;
using Consul;
using System;

namespace Core.ServiceDiscovery
{
    public class ConsulModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            //builder.RegisterIfNot<IServiceDiscoveryHelper, ServiceDiscoveryHelper>(ServiceLifetime.Singleton);

            builder.Register(c => 
            {
                var serviceDiscoveryConfiguration = c.Resolve<ServiceDiscoveryConfiguration>();
                return new ConsulClient(config => config.Address = new Uri(serviceDiscoveryConfiguration.Address));
            })
            .AsSelf()
            .As<IConsulClient>()
            .IfNotRegistered(typeof(IConsulClient))
            .SingleInstance();

            builder.RegisterType<KV>()
            .AsSelf()
            .As<IKVEndpoint>()
            .IfNotRegistered(typeof(IKVEndpoint))
            .SingleInstance();
        }
    }
}
