using System.Threading.Tasks;

namespace Core.Messages
{
    public class NullMessagePublisherWrapper : IMessagePublisherWrapper
    {
        public void Publish(IMessageWrapper messageWrapper)
        {
            // No Actions.
        }

        public Task PublishAsync(IMessageWrapper messageWrapper)
        {
            // No Actions.
            return Task.CompletedTask;
        }

        public static NullMessagePublisherWrapper Instance => new NullMessagePublisherWrapper();
    }
}