using Core.Messages.Factories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Messages.Bus
{
    public class NullMessageBus : IMessageBus
    {
        Task IMessageBus.OnMessageReceivedAsync(IMessage message, IRichMessageDescriptor descriptor)
        {
            return Task.CompletedTask;
        }

        Task IMessageBus.PublishAsync<T>(T message)
        {
            return Task.CompletedTask;
        }

        IDisposable IMessageBus.Register(Type messageType, IMessageHandlerFactory messageHandlerFactory)
        {
            return new NullDisposableObject();
        }


        void IMessageBus.Unregister(Type messageType, IMessageHandlerFactory factory)
        {
            // No Actions.
        }

        IEnumerable<Type> IMessageBus.GetAllHandledMessageTypes()
        {
            return Enumerable.Empty<Type>();
        }

        public static NullMessageBus Instance => new NullMessageBus();
    }

    internal class NullDisposableObject : IDisposable
    {
        public void Dispose()
        {
            // No Actions.
        }
    }
}