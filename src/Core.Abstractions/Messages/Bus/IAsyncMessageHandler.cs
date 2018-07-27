using System.Threading.Tasks;

namespace Core.Messages
{
    public interface IAsyncMessageHandler<in TMessage> : IMessageHandler where TMessage : IMessage
    {
        Task HandleMessageAsync(TMessage message);
    }
}