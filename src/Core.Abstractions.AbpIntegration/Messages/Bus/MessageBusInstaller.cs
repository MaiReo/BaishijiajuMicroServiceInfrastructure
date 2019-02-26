using Abp.Dependency;
using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Core.Messages.Bus.Factories;
using Core.Messages.Bus.Internal;

namespace Core.Messages.Bus
{
    internal class MessageBusInstaller : IWindsorInstaller
    {
        private IMessageHandlerFactoryStore _messageHandlerFactoryStore;

        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(
                Component
                .For<IMessageHandlerFactoryStore, MessageHandlerFactoryStore>()
                .ImplementedBy<MessageHandlerFactoryStore>()
                .LifestyleSingleton(),
                Component
                .For<IMessageHandlerCaller,ExpressionTreeMessageHandlerCaller>()
                .ImplementedBy<ExpressionTreeMessageHandlerCaller>()
                .LifestyleSingleton(),
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
            foreach (var descriptor in handler.ComponentModel.Implementation.GetMessageHandlerDescriptors())
            {
                _messageHandlerFactoryStore.Register(descriptor.MessageType, new IocMessageHandlerFactory(descriptor));
            }
        }
    }
}