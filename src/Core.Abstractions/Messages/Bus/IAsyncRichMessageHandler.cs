using System.Threading.Tasks;

namespace Core.Messages
{
    public interface IAsyncRichMessageHandler<in TMessage> : IMessageHandler where TMessage : IMessage
    {
        Task HandleMessageAsync(TMessage message, IRichMessageDescriptor descriptor);
    }
}