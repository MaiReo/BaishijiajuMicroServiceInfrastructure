using Core.Session;
using Core.Session.Extensions;
using System.Threading.Tasks;

namespace Core.Messages
{
    public class MessagePublisher : IMessagePublisher
    {
        private readonly IMessageDescriptorResolver _messageTopicResolver;
        private readonly ICoreSessionProvider _coreSessionProvider;
        private readonly IMessagePublisherWrapper _messagePublisherWrapper;

        public MessagePublisher(
                IMessageDescriptorResolver messageTopicResolver,
                ICoreSessionProvider coreSessionProvider,
                IMessagePublisherWrapper messagePublisherWrapper)
        {
            _messageTopicResolver = messageTopicResolver;
            _coreSessionProvider = coreSessionProvider;
            _messagePublisherWrapper = messagePublisherWrapper;
        }

        public virtual void Publish<T>(T message) where T : IMessage
        {
            var descriptor = _messageTopicResolver.Resolve(message);

            var headers = _coreSessionProvider.Session.ToHeaders();

            if (headers != null)
            {
                var rich = new RichMessageDescriptor(descriptor.MessageGroup, descriptor.MessageTopic);
                foreach (var item in headers)
                {
                    rich.Headers.Add(item.Key, item.Value);
                }
                descriptor = rich;
            }

            var wrapper = new MessageWrapper(descriptor, message);
            _messagePublisherWrapper.Publish(wrapper);
        }

        public virtual async Task PublishAsync<T>(T message) where T : IMessage
        {
            var descriptor = _messageTopicResolver.Resolve(message);
            var headers = _coreSessionProvider.Session.ToHeaders();

            if (headers != null)
            {
                var rich = new RichMessageDescriptor(descriptor.MessageGroup, descriptor.MessageTopic);
                foreach (var item in headers)
                {
                    rich.Headers.Add(item.Key, item.Value);
                }
                descriptor = rich;
            }
            var wrapper = new MessageWrapper(descriptor, message);
            await _messagePublisherWrapper.PublishAsync(wrapper);
        }
    }
}