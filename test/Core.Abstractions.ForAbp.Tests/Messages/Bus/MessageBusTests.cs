using Abp.Dependency;
using Core.Abstractions.Tests.Messages;
using Core.Messages;
using Core.Session;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Core.Abstractions.Tests
{
    public class TestMessageHandler : IMessageHandler<TestMessage>, ISingletonDependency
    {
        public TestMessageHandler()
        {
            _parameters = new List<TestMessage>();
        }
        public List<TestMessage> _parameters;
        public IReadOnlyCollection<TestMessage> Parameters => _parameters;
        public void HandleMessage(TestMessage message)
        {
            _parameters.Add(message);
        }
    }

    public class TestRichMessageHandler : IRichMessageHandler<TestMessage>, ISingletonDependency
    {
        public TestRichMessageHandler()
        {
            _parameters = new List<(TestMessage, IMessageDescriptor)>();
        }
        public List<(TestMessage, IMessageDescriptor)> _parameters;
        public IReadOnlyCollection<(TestMessage message, IMessageDescriptor descriptor)> Parameters => _parameters;
        public void HandleMessage(TestMessage message, IRichMessageDescriptor descriptor)
        {
            _parameters.Add((message, descriptor));
        }
    }

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

        [Fact(DisplayName = "Abp消息总线处理消息")]
        public async Task Should_Trigger()
        {
            await MessageBus.OnMessageReceivedAsync(new TestMessage {  TestTitle = "testmsg" }, new RichMessageDescriptor("test.group", "test.topic"));
            Resolve<TestMessageHandler>().Parameters.Single(x => x.TestTitle == "testmsg");
        }

        [Fact(DisplayName = "Abp消息总线处理富消息")]
        public async Task Should_Trigger_Rich()
        {
            await MessageBus.OnMessageReceivedAsync(new TestMessage {  TestTitle = "testmsg" }, new RichMessageDescriptor("test.group", "test.topic"));
            Resolve<TestRichMessageHandler>().Parameters.Single(x => x.message.TestTitle == "testmsg" && x.descriptor.MessageTopic == "test.topic");
        }
    }
}
