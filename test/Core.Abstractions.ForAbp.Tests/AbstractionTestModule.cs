using Abp.Modules;
using Abp.Reflection.Extensions;
using Abp.TestBase;
using Castle.MicroKernel.Registration;
using Core.Abstractions.Tests.Fakes;
using Core.Messages;
using Core.Messages.Bus;
using Core.Messages.Store;
using Core.Session;
using System.Net.Http;

namespace Core.Abstractions.Tests
{
    [DependsOn(
        typeof(AbstractionModule),
        typeof(AbpTestBaseModule)
        )]
    public class AbstractionTestModule : AbpModule
    {
        public override void PreInitialize()
        {
            Configuration.UnitOfWork.IsTransactional = false; //EF Core InMemory DB does not support transactions.
            Configuration.BackgroundJobs.IsJobExecutionEnabled = false;
            SetupFakeServices();
        }

        private void SetupFakeServices()
        {
            IocManager.IocContainer.Register(
                    Component
                    .For<HttpMessageHandler>()
                    .Instance(FakeHttpMessageHandler.Instance)
                    .NamedAutomatically(nameof(FakeHttpMessageHandler))
                    .IsDefault(),

                    Component
                    .For<IConsumedMessageStorageProvider>()
                    .ImplementedBy<FakeConsumedMessageStorageProvider>()
                    .NamedAutomatically(nameof(FakeConsumedMessageStorageProvider))
                    .IsDefault(),

                     Component
                    .For<IPublishedMessageStorageProvider>()
                    .ImplementedBy<FakePublishedMessageStorageProvider>()
                    .NamedAutomatically(nameof(FakePublishedMessageStorageProvider))
                    .IsDefault(),

                    Component
                    .For<UnitTestCurrentUser>()
                    .ImplementedBy<UnitTestCurrentUser>()
                    .LifestyleSingleton(),

                     Component
                    .For<ICoreSessionProvider>()
                    .ImplementedBy<UnitTestCoreSessionProvider>()
                    .LifestyleSingleton()
                    .IsFallback(),

                    Component
                    .For<IMessageBusOptions>()
                    .Instance(new MessageBusOptions
                    {
                        ExchangeName = TestConsts.MESSAGE_BUS_EXCHANGE,
                        HostName = TestConsts.MESSAGE_BUS_HOST,
                        Password = TestConsts.MESSAGE_BUS_PWD,
                        QueueName = TestConsts.MESSAGE_BUS_QUEUE,
                        UserName = TestConsts.MESSAGE_BUS_USER,
                        VirtualHost = TestConsts.MESSAGE_BUS_VHOST
                    })
                    .NamedAutomatically(nameof(MessageBusOptions))
                    .IsDefault()
                    );
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(AbstractionTestModule).GetAssembly());
            if (!IocManager.IsRegistered<IMessagePublisherWrapper>())
            {
                IocManager.IocContainer.Register(
                    Component
                    .For<IMessagePublisherWrapper>()
                    .Instance(FakeMessagePublisherWrapper.Instance)
                    .NamedAutomatically(nameof(FakeMessagePublisherWrapper))
                    .IsDefault()
                );
            }
        }

    }
}