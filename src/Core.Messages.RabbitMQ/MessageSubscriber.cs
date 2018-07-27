using Core.Messages.Bus;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Core.Messages
{
    public class MessageSubscriber : IMessageSubscriber
    {
        private readonly IMessageBus _messageBus;
        private readonly IRabbitMQWrapper _rabbitMQWrapper;
        private readonly IMessageDescriptorResolver _messageTopicResolver;

        private bool _isAutoSubscribed;

        public MessageSubscriber(IMessageBus messageBus,
            IRabbitMQWrapper rabbitMQWrapper,
            IMessageDescriptorResolver messageTopicResolver,
            ILogger<MessageSubscriber> logger)
        {
            this._messageBus = messageBus;
            this._rabbitMQWrapper = rabbitMQWrapper;
            this._messageTopicResolver = messageTopicResolver;
            Logger = (ILogger)logger ?? NullLogger.Instance;
        }

        public ILogger Logger { get; }

        public void AutoSubscribe()
        {
            if (_isAutoSubscribed)
            {
                throw new System.InvalidOperationException("already automatic subscribed");
            }
            var messageTypes = this._messageBus.GetAllHandledMessageTypes();

            foreach (var messageType in messageTypes)
            {
                var descriptor = this._messageTopicResolver.Resolve(messageType);
                Logger.LogInformation($"AutoSubscribe: Found messageType:{messageType}, topic name: {descriptor?.MessageTopic}, group name: {descriptor?.MessageGroup}");
                this._rabbitMQWrapper.Subscribe(descriptor, async msg => await this._messageBus.OnMessageReceivedAsync(msg));
            }
            _isAutoSubscribed = true;

        }
    }
}