using Abp.Dependency;
using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Core.Messages.Bus.Factories;

namespace Core.Messages.Bus
{
    internal class MessageBusInstaller : IWindsorInstaller
    {
        private readonly IIocResolver _iocResolver;
        private IMessageHandlerFactoryStore  _messageHandlerFactoryStore;

        public MessageBusInstaller(IIocResolver iocResolver)
        {
            _iocResolver = iocResolver;
        }

        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(
                Component
                .For<IMessageHandlerFactoryStore, MessageHandlerFactoryStore>()
                .ImplementedBy<MessageHandlerFactoryStore>()
                .LifestyleSingleton()
            );
            container.Register(
                Component
                .For<IMessageBus, MessageBus>()
                .ImplementedBy<MessageBus>()
                .LifestyleTransient()
            );
            _messageHandlerFactoryStore = container.Resolve<IMessageHandlerFactoryStore>();
            container.Kernel.ComponentRegistered += RegisterMessageHandler;
        }

        private void RegisterMessageHandler(string key, IHandler handler)
        {
            /* This code checks if registering component implements any IMessageHandler<in TMessage> interface, if yes,
             * gets all message handler interfaces and registers type to Message Bus for each handling message.
             */
            if (!typeof(IMessageHandler).IsAssignableFrom(handler.ComponentModel.Implementation))
            {
                return;
            }
            var interfaces = handler.ComponentModel.Implementation.GetInterfaces();
            foreach (var @interface in interfaces)
            {
                if (!typeof(IMessageHandler).IsAssignableFrom(@interface))
                {
                    continue;
                }
                if (!@interface.IsGenericType)
                {
                    continue;
                }
                var genericArgs = @interface.GetGenericArguments();
                if (genericArgs.Length == 1)
                {
                    _messageHandlerFactoryStore.Register(genericArgs[0], new IocMessageHandlerFactory(handler.ComponentModel.Implementation));
                }
            }
        }
    }
}