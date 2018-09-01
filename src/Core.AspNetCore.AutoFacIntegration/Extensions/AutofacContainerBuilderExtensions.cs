using Core.Abstractions;
using Core.Messages;
using Core.PersistentStore;
using Core.ServiceDiscovery;

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
        }
    }
}
