using Core.Abstractions;
using Core.Messages.Factories;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Core.Messages.Bus
{
    public class MessageHandlerFactoryStore : IMessageHandlerFactoryStore
    {
        /// <summary>
        /// All registered handler factories.
        /// Key: Type of the event
        /// Value: List of handler factories
        /// </summary>
        private readonly ConcurrentDictionary<Type, ICollection<IMessageHandlerFactory>> _handlerFactories;

        public MessageHandlerFactoryStore()
        {
            _handlerFactories = new ConcurrentDictionary<Type, ICollection<IMessageHandlerFactory>>();
        }

        /// <inheritdoc/>
        public IDisposable Register(Type messageType, IMessageHandlerFactory factory)
        {
            GetOrCreateHandlerFactories(messageType)
                .Locking(factories => factories.Add(factory));
            return new MessageHandlerFactoryUnregistrar(this, messageType, factory);
        }

        private ICollection<IMessageHandlerFactory> GetOrCreateHandlerFactories(Type eventType)
        {
            return _handlerFactories.GetOrAdd(eventType, (type) => new HashSet<IMessageHandlerFactory>(new MessageHandlerFactoryUniqueComparer()));
        }

        public void Unregister(Type messageType, IMessageHandlerFactory factory)
        {
            GetOrCreateHandlerFactories(messageType).Locking(factories => factories.Remove(factory));
        }

        public IEnumerable<KeyValuePair<Type, IEnumerable<IMessageHandlerFactory>>> GetHandlerFactories()
        {
            foreach (var handlerFactory in this._handlerFactories)
            {
                yield return new KeyValuePair<Type, IEnumerable<IMessageHandlerFactory>>(handlerFactory.Key, handlerFactory.Value);
            }
        }

        public IEnumerable<Type> GetAllHandledMessageTypes()
        {
            return _handlerFactories.Keys;
        }

        public static MessageHandlerFactoryStore Instance { get; }

        static MessageHandlerFactoryStore()
        {
            Instance = new MessageHandlerFactoryStore();
        }
    }

    internal class MessageHandlerFactoryUniqueComparer : IEqualityComparer<IMessageHandlerFactory>
    {
        public bool Equals(IMessageHandlerFactory x, IMessageHandlerFactory y)
        {
            if (x == null && y == null) return true;
            if (x == null && y != null) return false;
            if (x != null && y == null) return false;
            return x.GetHandlerType() == y.GetHandlerType();
        }

        public int GetHashCode(IMessageHandlerFactory obj)
        {
            return obj?.GetHandlerType()?.GetHashCode() ?? 0;
        }
    }
}