using Abp.TestBase;
using Castle.MicroKernel.Registration;
using Core.Messages;
using Core.Messages.Bus;
using Core.Session;

namespace Core.Abstractions.Tests
{
    public class AbstractionTestBase : AbpIntegratedTestBase<AbstractionTestModule>
    {
        public AbstractionTestBase()
        {
            LoginAsDefaultUser();
        }
        private void LoginAsDefaultUser()
        {
            LoginAs(TestConsts.CURRENT_USER_ID, TestConsts.CURRENT_USER_NAME);
        }

        protected void LoginAs(string userId, string userName)
        {
            Resolve<UnitTestCurrentUser>().Set(userId, userName);
        }

        protected ICoreSessionContainer<string, string> CurrentUser => Resolve<UnitTestCurrentUser>();

        protected virtual bool UseDomainMessagePublisher { get; }

        protected override void PreInitialize()
        {
            if (UseDomainMessagePublisher)
            {
                LocalIocManager.IocContainer.Register(
                   Component
                   .For<IMessagePublisherWrapper>()
                   .ImplementedBy<DomainMessagePublisherWrapper>()
                   .NamedAutomatically(nameof(DomainMessagePublisherWrapper))
                   .LifestyleTransient()
                   .IsDefault()
               );
            }
        }

        public IMessageBus MessageBus { get; private set; }

        protected override void PostInitialize()
        {
            MessageBus = Resolve<IMessageBus>();
        }
    }
}