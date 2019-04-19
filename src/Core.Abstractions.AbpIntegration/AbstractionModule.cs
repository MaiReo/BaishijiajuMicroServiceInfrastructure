using Abp.Dependency;
using Castle.MicroKernel.Registration;
using Core.BackgroundJobs;
using Core.DependencyRegistrars;
using Core.Messages;
using Core.Messages.Bus;
using Core.Messages.Store;
using Core.Messages.Utilities;
using Core.RemoteCall;
using Core.ServiceDiscovery;
using Core.Session;
using Core.Utilities;
using Core.Wrappers;
using System.Net.Http;

namespace Abp.Modules
{
    public class AbstractionModule : AbpModule
    {
        public override void PreInitialize()
        {
            IocManager.AddConventionalRegistrar(new MessageHandlerConventionalRegistrar());
            IocManager.IocContainer.Install(new MessageBusInstaller());
            IocManager.RegisterIfNot<RandomServiceEndpointSelector>();
            IocManager.RegisterIfNot<IMessageHasher, MessageHasher>();
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(AbstractionModule).Assembly);
        }

        public override void PostInitialize()
        {
            IocManager.RegisterIfNot<ServiceDiscoveryConfiguration>();
            IocManager.RegisterIfNot<IMessagePublisher, MessagePublisher>(DependencyLifeStyle.Transient);
            IocManager.RegisterIfNot<IMessageDescriptorResolver, MessageDescriptorResolver>();
            IocManager.RegisterIfNot<IMessageConverter, DefaultMessageConverter>();
            IocManager.RegisterIfNot<IMessageScopeCreator, NullMessageScopeCreator>();
            IocManager.RegisterIfNot<HttpMessageHandler, HttpClientHandler>();
            IocManager.RegisterIfNot<IHttpClientWrapper, HttpClientWrapper>();
            IocManager.RegisterIfNot<IServiceHelper, DefaultServiceHelper>();
            IocManager.RegisterIfNot<IRPCService, RPCService>(DependencyLifeStyle.Transient);
            IocManager.RegisterIfNot<IBackgroundJobHelper, NullBackgroundJobHelper>();
            IocManager.RegisterIfNot<IServiceHelper, NullServiceHelper>();
            IocManager.RegisterIfNot<IServiceDiscoveryHelper, NullServiceDiscoveryHelper>();
            if (!IocManager.IsRegistered<HttpClient>())
            {
                IocManager.IocContainer.Register(
                   Component
                   .For<HttpClient>()
                   .UsingFactoryMethod(krnl => krnl.Resolve<IHttpClientWrapper>().HttpClient)
                   );
            }
            IocManager.RegisterIfNot<IMessageSubscriber, NullMessageSubscriber>();
            IocManager.RegisterIfNot<IMessagePublisherWrapper, NullMessagePublisherWrapper>();
            IocManager.RegisterIfNot<IServiceDiscoveryHelper, NullServiceDiscoveryHelper>();
            IocManager.RegisterIfNot<ICoreSessionProvider, NullCoreSessionProvider>();
            if (!IocManager.IsRegistered<ICoreSession>())
            {
                IocManager.IocContainer.Register(
                   Component
                   .For<ICoreSession>()
                   .UsingFactoryMethod(krnl => krnl.Resolve<ICoreSessionProvider>().Session)
                   );
            }
            if (!IocManager.IsRegistered<IServiceEndpointSelector>())
            {
                IocManager.IocContainer.Register(
                   Component
                   .For<IServiceEndpointSelector>()
                   .ImplementedBy<RandomServiceEndpointSelector>()
                   .NamedAutomatically(nameof(RandomServiceEndpointSelector))
                   .LifestyleSingleton()
                   );
            }
            IocManager.RegisterIfNot<IPublishedMessageStore, PublishedMessageStore>(DependencyLifeStyle.Transient);

            IocManager.RegisterIfNot<IConsumedMessageStore, ConsumedMessageStore>(DependencyLifeStyle.Transient);

            IocManager.RegisterIfNot<IPublishedMessageStorageProvider, PublishedLoggerMessageStorageProvider>();

            IocManager.RegisterIfNot<IConsumedMessageStorageProvider, ConsumedLoggerMessageStorageProvider>();

        }
    }
}