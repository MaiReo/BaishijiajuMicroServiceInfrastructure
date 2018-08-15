using Core.Messages.Bus;
using System;

namespace Core.Messages.Factories
{
    /// <summary>
    /// Used to unregister a <see cref="IMessageHandlerFactory"/> on <see cref="Dispose"/> method.
    /// </summary>
    internal class MessageHandlerFactoryUnregistrar : IDisposable
    {
       

        private readonly IMessageBus _messageBus;
        private readonly Type _messageType;
        private readonly IMessageHandlerFactory _factory;

        public MessageHandlerFactoryUnregistrar(IMessageBus messageBus, Type messageType, IMessageHandlerFactory factory)
        {
            this._messageBus = messageBus;
            this._messageType = messageType;
            this._factory = factory;
        }

        public void Dispose()
        {
            this._messageBus.Unregister(_messageType, _factory);
        }
    }
}