using Core.Messages.Factories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Messages.Bus
{
    public interface IMessageBus
    {
        Task PublishAsync<T>(T message) where T : IMessage;

        Task OnMessageReceivedAsync(IMessage message, IRichMessageDescriptor descriptor);

        IDisposable Register(Type messageType, IMessageHandlerFactory factory);

        void Unregister(Type messageType, IMessageHandlerFactory factory);

        IEnumerable<Type> GetAllHandledMessageTypes();
    }
}