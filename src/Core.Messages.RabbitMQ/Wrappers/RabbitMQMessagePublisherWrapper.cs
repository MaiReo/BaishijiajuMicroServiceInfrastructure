using System.Threading.Tasks;

namespace Core.Messages
{
    public class RabbitMQMessagePublisherWrapper : IMessagePublisherWrapper
    {
        private readonly IRabbitMQWrapper _rabbitMQWrapper;
        private readonly IMessageConverter _messageConverter;

        public RabbitMQMessagePublisherWrapper(
            IRabbitMQWrapper rabbitMQWrapper,
            IMessageConverter messageConverter)
        {
            this._rabbitMQWrapper = rabbitMQWrapper;
            this._messageConverter = messageConverter;
        }

        public void Publish(IMessageWrapper messageWrapper)
        {
            _rabbitMQWrapper.Publish(messageWrapper.Descriptor,
                _messageConverter.Serialize(messageWrapper.Message));
        }

        public ValueTask PublishAsync(IMessageWrapper messageWrapper)
        {
            return _rabbitMQWrapper.PublishAsync(messageWrapper.Descriptor,
                _messageConverter.Serialize(messageWrapper.Message));
        }
    }
}