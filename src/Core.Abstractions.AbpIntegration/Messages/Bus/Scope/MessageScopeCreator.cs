using Abp.Dependency;
using System;

namespace Core.Messages.Bus
{
    public class MessageScopeCreator : IMessageScopeCreator, ISingletonDependency
    {
        private readonly IIocResolver _iocResolver;

        public MessageScopeCreator(IIocResolver iocResolver)
        {
            _iocResolver = iocResolver;
        }

        public IMessageScope CreateScope(IMessage message, IRichMessageDescriptor messageDescriptor)
        {
            //TODO:
            // How can we resolve MessageCoreSessionProvider as ICoreSessionProvider?
            // Well, try adding a ChildContainer and remove it on disposing?
            // No.Hacking the global IOC is dangerous.
            return new MessageScope(_iocResolver);
        }
    }
}
