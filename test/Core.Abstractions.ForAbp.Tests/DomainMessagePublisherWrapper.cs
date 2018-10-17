using Abp.Dependency;
using Core.Messages;
using Core.Messages.Bus;
using System.Threading.Tasks;

namespace Core.Abstractions.Tests
{
    public class DomainMessagePublisherWrapper : IMessagePublisherWrapper
    {
        private readonly IIocResolver _iocResolver;

        public DomainMessagePublisherWrapper(IIocResolver iocResolver)
        {
            _iocResolver = iocResolver;
        }

        public void Publish(IMessageWrapper messageWrapper)
        {
            var messageBus = _iocResolver.Resolve<IMessageBus>();
            messageBus
                .OnMessageReceivedAsync(messageWrapper.Message, messageWrapper.Descriptor as IRichMessageDescriptor)
                .GetAwaiter()
                .GetResult()
            ;
        }

        public Task PublishAsync(IMessageWrapper messageWrapper)
        {
            var messageBus = _iocResolver.Resolve<IMessageBus>();
            return messageBus.OnMessageReceivedAsync(messageWrapper.Message, messageWrapper.Descriptor as IRichMessageDescriptor);
        }

    }
}
