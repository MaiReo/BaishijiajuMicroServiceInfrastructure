using Autofac;
using Core.Messages.Bus;
using Core.ServiceDiscovery;
using Core.Session;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Abstractions.TestBase
{
    public abstract class AbstractionTestBase<TStartup> where TStartup :class
    {
        private readonly ContainerBuilder _containerBuilder;
        protected virtual IContainer IocContainer { get; }
        public AbstractionTestBase()
        {
            var services = new ServiceCollection();
            _containerBuilder = RegisterRequiredServices(services);
            IocContainer = _containerBuilder.Build();
            ConstructProperties();
        }

        public IMessageBus MessageBus { get; private set; }

        protected internal virtual ContainerBuilder RegisterRequiredServices(IServiceCollection services)
        {
            services.AddDistributedMemoryCache();
            services.RegisterRequiredServices<TStartup>();
            services.AddMessageBus(o =>
            {
                o.ExchangeName = TestConsts.MESSAGE_BUS_EXCHANGE;
                o.HostName = TestConsts.MESSAGE_BUS_HOST;
                o.Password = TestConsts.MESSAGE_BUS_PWD;
                o.QueueName = TestConsts.MESSAGE_BUS_QUEUE;
                o.UserName = TestConsts.MESSAGE_BUS_USER;
                o.VirtualHost = TestConsts.MESSAGE_BUS_VHOST;
            });
            services.AddServiceDiscovery(o => o.Address = ServiceDiscoveryConfiguration.DEFAULT_ADDRESS);

            var containerBuilder = services.AddAutoFacWithConvention<TStartup>();
            containerBuilder.RegisterAssemblyByConvention(typeof(AbstractionTestBase<>).Assembly);
            containerBuilder.RegisterAssemblyByConvention(this.GetType().Assembly);
            containerBuilder
                .RegisterInstance(TestSession.Instance)
                .As<ICoreSession>();

            RegisterDependency(containerBuilder);
            return containerBuilder;
        }

        /// <summary>
        /// To register something needed for tests, override this method.
        /// </summary>
        /// <param name="builder"></param>
        protected virtual void RegisterDependency(ContainerBuilder builder)
        {
            // classes inhert from this my override this method to register something needed.
        }

        private void ConstructProperties()
        {
            MessageBus = Resolve<IMessageBus>();
        }

        /// <summary>
        /// Shortcut for resolving a object from IocContainer.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        protected T Resolve<T>() => IocContainer.Resolve<T>();
    }
}
