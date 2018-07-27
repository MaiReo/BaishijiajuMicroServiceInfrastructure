using System.Threading.Tasks;

namespace Core.Messages
{
    public class MessagePublisher : IMessagePublisher
    {
        private readonly IMessageDescriptorResolver _messageTopicResolver;
        private readonly IMessagePublisherWrapper _messagePublisherWrapper;

        public MessagePublisher(
                IMessageDescriptorResolver messageTopicResolver,
                IMessagePublisherWrapper messagePublisherWrapper)
        {
            this._messageTopicResolver = messageTopicResolver;
            this._messagePublisherWrapper = messagePublisherWrapper;
        }

        public virtual void Publish<T>(T message) where T : IMessage
        {
            var descriptor = _messageTopicResolver.Resolve(message);
            var wrapper = new MessageWrapper(descriptor, message);
            this._messagePublisherWrapper.Publish(wrapper);
        }

        public async virtual Task PublishAsync<T>(T message) where T : IMessage
        {
            var descriptor = _messageTopicResolver.Resolve(message);
            var wrapper = new MessageWrapper(descriptor, message);
            await this._messagePublisherWrapper.PublishAsync(wrapper);
        }
    }
}