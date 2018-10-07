using Core.Abstractions.Tests.Messages;
using Core.Session;
using Shouldly;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Core.Abstractions.Tests
{
    public class MessageBusTests : AbstractionTestBase
    {

        protected override bool UseDomainMessagePublisher => true;


        [Fact(Skip = "Failed to override registrations with Windsor Child Kernel facility.")]
        public async Task MessageBusRecursiveScopeTest()
        {
            var rootCoreSessionProvider = Resolve<ICoreSessionProvider>();

            rootCoreSessionProvider.ShouldBeOfType<UnitTestCoreSessionProvider>();

            using ((rootCoreSessionProvider as UnitTestCoreSessionProvider).Use(TestConsts.CITY_ID,Guid.Parse(TestConsts.COMPANY_ID),TestConsts.COMPANY_NAME,Guid.Parse(TestConsts.STORE_ID),TestConsts.STORE_NAME,TestConsts.BROKER_ID,TestConsts.BROKER_NAME))
            {
                var message = new TestMessage2();

                await MessageBus.PublishAsync(message);

                //No exception(s) means test was passed.
            }



        }
    }
}
