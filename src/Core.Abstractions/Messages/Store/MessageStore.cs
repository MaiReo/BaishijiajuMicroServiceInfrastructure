using Core.Messages.Utilities;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Messages.Store
{
    public abstract class MessageStore : IMessageStore
    {
        private readonly IMessageStorageProvider _storageProvider;
        private readonly IMessageHasher _messageHasher;
        private readonly IMessageConverter _messageConverter;

        public MessageStore(
            IMessageStorageProvider storageProvider,
            IMessageHasher messageHasher,
            IMessageConverter messageConverter)
        {
            _storageProvider = storageProvider;
            _messageHasher = messageHasher;
            _messageConverter = messageConverter;
        }

        protected async ValueTask<bool> IsExistsAsync(IMessageDescriptor descriptor, IMessage message, CancellationToken cancellationToken = default)
        {
            var hash = await _messageHasher.HashAsync(descriptor, message, cancellationToken: cancellationToken);
            var model = await _storageProvider.FindAsync(hash, cancellationToken);
            return
                 string.Equals(model.Group, descriptor?.MessageGroup)
                 &&
                 string.Equals(model.Topic, descriptor?.MessageTopic);
        }

        public async ValueTask StoreAsync(IMessageDescriptor descriptor, IMessage message, CancellationToken cancellationToken = default)
        {
            if (descriptor == null)
            {
                throw new System.ArgumentNullException(nameof(descriptor));
            }
            if (message == null)
            {
                throw new System.ArgumentNullException(nameof(message));
            }
            var hash = await _messageHasher.HashAsync(descriptor, message, cancellationToken: cancellationToken);
            var messageString = _messageConverter.SerializeString(message);
            var typeName = message.GetType().AssemblyQualifiedName;
            var model = new MessageModel(typeName, messageString, hash, descriptor.MessageGroup, descriptor.MessageTopic);
            await _storageProvider.SaveAsync(model, cancellationToken);
        }
    }
}
