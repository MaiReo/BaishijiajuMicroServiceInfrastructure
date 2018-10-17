using Abp.Dependency;
using Castle.MicroKernel.Registration;
using Core.DependencyRegistrars;
using Core.Messages;
using Core.Messages.Bus;
using Core.ServiceDiscovery;
using Core.Session;
using Core.Wrappers;
using System.Net.Http;

namespace Abp.Modules
{
    public class AbstractionModule : AbpModule
    {
        public override void PreInitialize()
        {
            IocManager.AddConventionalRegistrar(new MessageHandlerConventionalRegistrar());
            IocManager.IocContainer.Install(new MessageBusInstaller(IocManager));

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
            IocManager.RegisterIfNot<IMessageBus, MessageBus>();
            IocManager.RegisterIfNot<HttpMessageHandler, HttpClientHandler>();
            IocManager.RegisterIfNot<IHttpClientWrapper, HttpClientWrapper>();
            IocManager.RegisterIfNot<IServiceHelper, DefaultServiceHelper>();
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
        }
    }
}