using System;
using System.Threading.Tasks;

namespace Core.Messages
{
    public interface IRabbitMQWrapper
    {
        void Publish(IMessageDescriptor descriptor, byte[] message);

        Task PublishAsync(IMessageDescriptor descriptor, byte[] message);

        void Subscribe(IMessageDescriptor descriptor, Action<IMessage> handler);
        void Subscribe(IMessageDescriptor descriptor, Action<IMessage, IRichMessageDescriptor> handler);

        void Subscribe(IMessageDescriptor descriptor, Func<IMessage, Task> asyncHandler);

        void Subscribe(IMessageDescriptor descriptor, Func<IMessage, IRichMessageDescriptor, Task> asyncHandler);

        void UnSubscribe(IMessageDescriptor descriptor);
    }
}