using Core.Messages.Bus;
using Core.Messages.Bus.Factories;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;

namespace Core.Messages
{
    public class DefaultMessageConverter : IMessageConverter
    {
        private readonly IMessageHandlerFactoryStore _messageHandlerFactoryStore;
        private readonly IMessageDescriptorResolver _messageTopicResolver;

        public DefaultMessageConverter(
            IMessageHandlerFactoryStore  messageHandlerFactoryStore,
            IMessageDescriptorResolver messageTopicResolver)
        {
            _messageHandlerFactoryStore = messageHandlerFactoryStore;
            _messageTopicResolver = messageTopicResolver;
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
            var allMessageTypes = _messageHandlerFactoryStore.GetAllHandledMessageTypes();
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

            var typedMessageObject = JsonConvert.DeserializeObject(stringMessage, type) as IMessage;

            return typedMessageObject;
        }

        public byte[] Serialize(IMessage message)
        {
            var stringMessage = SerializeString(message);
            var raw = Encoding.UTF8.GetBytes(stringMessage);
            return raw;
        }

        public string SerializeString(IMessage message)
        {
            var stringMessage = "{}";
            if (message != null)
            {
                stringMessage = JsonConvert.SerializeObject(message);
            }
            return stringMessage;
        }
        public string DeSerializeString(byte[] message)
        {
            return Encoding.UTF8.GetString(message);
        }
    }
}