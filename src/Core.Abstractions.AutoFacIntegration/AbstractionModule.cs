using Autofac;
using Autofac.Core;
using Core.BackgroundJobs;
using Core.Messages;
using Core.Messages.Bus;
using Core.Messages.Bus.Factories;
using Core.PersistentStore.Repositories;
using Core.RemoteCall;
using Core.ServiceDiscovery;
using Core.Session;
using Core.Wrappers;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Net.Http;

namespace Core.Abstractions
{
    public sealed class AbstractionModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterIfNot<IMessagePublisher, MessagePublisher>(ServiceLifetime.Transient)
                   .RegisterIfNot<IMessageDescriptorResolver, MessageDescriptorResolver>(ServiceLifetime.Transient)
                   .RegisterIfNot<IMessageScopeCreator, MessageScopeCreator>(ServiceLifetime.Singleton)
                   .RegisterIfNot<IMessageHandlerFactoryStore, MessageHandlerFactoryStore>(ServiceLifetime.Singleton)
                   .RegisterIfNot<IMessageBus, MessageBus>(ServiceLifetime.Transient)
                   .RegisterIfNot<IMessageConverter, DefaultMessageConverter>(ServiceLifetime.Singleton)
                   .RegisterIfNot<IServiceHelper, DefaultServiceHelper>(ServiceLifetime.Singleton)
                   .RegisterIfNot<HttpMessageHandler, HttpClientHandler>(ServiceLifetime.Singleton)
                   .RegisterIfNot<IHttpClientWrapper, HttpClientWrapper>(ServiceLifetime.Singleton)
                   .RegisterIfNot<IRPCService, RPCService>(ServiceLifetime.Singleton)
                   .RegisterIfNot<IBackgroundJobHelper, NullBackgroundJobHelper>(ServiceLifetime.Singleton)
                   .RegisterIfNot<ICoreSession, NullCoreSession>(ServiceLifetime.Singleton)
                   .RegisterIfNot<IServiceHelper, NullServiceHelper>(ServiceLifetime.Singleton)
                   .RegisterIfNot<IServiceDiscoveryHelper, NullServiceDiscoveryHelper>(ServiceLifetime.Singleton)
                   ;

            builder.Register(c => c.Resolve<IHttpClientWrapper>().HttpClient)
                .As<HttpClient>()
                .IfNotRegistered(typeof(HttpClient))
                .SingleInstance()
                .ExternallyOwned();

            //Consul
            builder.RegisterIfNot<ServiceDiscoveryConfiguration>();

            //Repository
            builder.RegisterGeneric(typeof(NullAsyncRepository<,>))
               .As(typeof(IRepository<,>))
               .IfNotRegistered(typeof(IRepository<,>))
               .SingleInstance();

            builder.RegisterGeneric(typeof(NullAsyncRepository<,>))
               .As(typeof(IAsyncRepository<,>))
               .IfNotRegistered(typeof(IAsyncRepository<,>))
               .SingleInstance();
        }
    }

}
