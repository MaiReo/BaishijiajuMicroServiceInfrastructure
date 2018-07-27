using Newtonsoft.Json;
using Core.Messages.Bus;
using System.Text;

namespace Core.Messages
{
    public class DefaultMessageConverter : IMessageConverter
    {
        private readonly IMessageBus _messageBus;
        private readonly IMessageDescriptorResolver _messageTopicResolver;

        public DefaultMessageConverter(
            IMessageBus messageBus,
            IMessageDescriptorResolver messageTopicResolver)
        {
            this._messageBus = messageBus;
            this._messageTopicResolver = messageTopicResolver;
        }

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
            var allMessageTypes = this._messageBus.GetAllHandledMessageTypes();
            var type = this._messageTopicResolver.Resolve(descriptor, allMessageTypes);
            if (type == null)
            {
                return null;
            }
            if (!typeof(IMessage).IsAssignableFrom(type))
            {
                return null;
            }
            var stringMessage = Encoding.UTF8.GetString(message);
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
            var raw = Encoding.UTF8.GetBytes(stringMessage);
            return raw;
        }
    }
}