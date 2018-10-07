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
            var name = nameof(MessageCoreSessionProvider);
            var coreSessionProvider = new MessageCoreSessionProvider(message, messageDescriptor);


            //TODO: The Child Kernel / Container has issues to use in this case.
            //Cannot resolve as a Dependency correctly.
            var childKernel = new DefaultKernel();

            childKernel.Register(
                Component
                .For<ICoreSessionProvider>()
                .Instance(coreSessionProvider)
                .NamedAutomatically(name)
                .LifestyleSingleton()
                .IsDefault()
                );

            return new MessageScope(_iocManager, childKernel);

        }

    }
}
