using Core.Messages.Bus;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;

namespace Core.Messages
{
    public class DefaultMessageConverter : IMessageConverter
    {
        private readonly IMessageBus _messageBus;
        private readonly IMessageDescriptorResolver _messageTopicResolver;

        public DefaultMessageConverter(
            IMessageBus messageBus,
            IMessageDescriptorResolver messageTopicResolver,
            ILogger<DefaultMessageConverter> logger)
        {
            _messageBus = messageBus;
            _messageTopicResolver = messageTopicResolver;
            Logger = logger;
        }

        public ILogger<DefaultMessageConverter> Logger { get; }

        public IMessage Deserialize(IMessageDescriptor descriptor, byte[] message)
        {
            if (descriptor == null)
            {
                return null;
            }
            if (string.IsNullOrWhiteSpace(descriptor.MessageTopic))
            {
                return null;
            }
            if (message == null || message.Length == 0)
            {
                return null;
            }
            var allMessageTypes = _messageBus.GetAllHandledMessageTypes();
            var type = _messageTopicResolver.Resolve(descriptor, allMessageTypes);
            if (type == null)
            {
                return null;
            }
            if (!typeof(IMessage).IsAssignableFrom(type))
            {
                return null;
            }
            var stringMessage = Encoding.UTF8.GetString(message);
            Logger.LogInformation($"[{descriptor.MessageGroup}][{descriptor.MessageTopic}]{stringMessage}");
            var typedMessageObject = JsonConvert.DeserializeObject(stringMessage, type) as IMessage;

            return typedMessageObject;
        }

        public byte[] Serialize(IMessage message)
        {
            var stringMessage = "{}";
            if (message != null)
            {
                stringMessage = JsonConvert.SerializeObject(message);
            }
            Logger.LogInformation(stringMessage);
            var raw = Encoding.UTF8.GetBytes(stringMessage);
            return raw;
        }
    }
}