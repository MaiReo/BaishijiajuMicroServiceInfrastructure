using Core.Abstractions;
using Core.Messages;
using Core.PersistentStore;
using Core.ServiceDiscovery;
using Core.Session;

namespace Autofac
{
    public static class AutofacContainerBuilderExtensions
    {
        /// <summary>
        /// Register modules to container.
        /// </summary>
        public static void AddCoreModules(this ContainerBuilder builder)
        {
            builder.RegisterModule<AbstractionModule>();
            builder.RegisterModule<RabbitMQModule>();
            builder.RegisterModule<ConsulModule>();
            builder.RegisterModule<EntityFrameworkCoreModule>();
            builder.RegisterAssemblyByConvention(typeof(AutofacContainerBuilderExtensions).Assembly);

            builder.RegisterType<HttpContextCoreSessionProvider>()
                .AsSelf()
                .As<ICoreSessionProvider>()
                .SingleInstance();

            builder.Register(c => c.Resolve<ICoreSessionProvider>().Session)
                .As<ICoreSession>();

            builder.RegisterType<ServiceHelper>()
                .AsSelf()
                .As<IServiceHelper>()
                .SingleInstance();

            builder.RegisterType<HealthCheckHelper>()
                .AsSelf()
                .As<IServiceHelper>()
                .SingleInstance();
        }
    }
}
