using Abp.Dependency;
using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Core.Session;
using Core.Session.Providers;

namespace Core.Messages.Bus
{
    public class MessageScopeCreator : IMessageScopeCreator, ISingletonDependency
    {
        private readonly IIocManager _iocManager;

        public MessageScopeCreator(IIocManager iocManager)
        {
            _iocManager = iocManager;
        }

        public IMessageScope CreateScope(IMessage message, IRichMessageDescriptor messageDescriptor)
        {
            var coreSessionProvider = new MessageCoreSessionProvider(message, messageDescriptor);

            return new MessageScope(_iocManager, coreSessionProvider);

        }

    }
}
