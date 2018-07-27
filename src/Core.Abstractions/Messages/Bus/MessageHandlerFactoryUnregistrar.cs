using Core.Messages.Bus;
using System;

namespace Core.Messages.Factories
{
    internal class MessageHandlerFactoryUnregistrar : IDisposable
    {
        /// <summary>
        /// Used to unregister a <see cref="IEventHandlerFactory"/> on <see cref="Dispose"/> method.
        /// </summary>

        private readonly IMessageHandlerFactoryStore _messageHandlerFactoryStore;
        private readonly Type _messageType;
        private readonly IMessageHandlerFactory _factory;

        public MessageHandlerFactoryUnregistrar(IMessageHandlerFactoryStore messageHandlerFactoryStore, Type messageType, IMessageHandlerFactory factory)
        {
            this._messageHandlerFactoryStore = messageHandlerFactoryStore;
            this._messageType = messageType;
            this._factory = factory;
        }

        public void Dispose()
        {
            this._messageHandlerFactoryStore.Unregister(_messageType, _factory);
        }
    }
}