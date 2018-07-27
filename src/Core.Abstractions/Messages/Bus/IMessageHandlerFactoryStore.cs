using Core.Messages.Factories;
using System;
using System.Collections.Generic;

namespace Core.Messages.Bus
{
    public interface IMessageHandlerFactoryStore
    {
        IDisposable Register(Type messageType, IMessageHandlerFactory factory);
        void Unregister(Type messageType, IMessageHandlerFactory factory);

        IEnumerable<KeyValuePair<Type, IEnumerable<IMessageHandlerFactory>>> GetHandlerFactories();

        IEnumerable<Type> GetAllHandledMessageTypes();
    }
}