using Core.Abstractions.Dependency;
using Core.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace Core.Abstractions.Tests
{

    public class TestMessage : IMessage
    {
        public string Name { get; set; }
    }

    public class TestMessageHandler : IMessageHandler<TestMessage>, ILifestyleSingleton
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

    public class MessageBusTests : TestBase.AbstractionTestBase<MessageBusTests>
    {

        [Fact(DisplayName = "消息总线处理消息")]
        public void Should_Trigger()
        {
            MessageBus.OnMessageReceived(new TestMessage { Name = "testmsg" });
            Resolve<TestMessageHandler>().Parameters.Single(x => x.Name == "testmsg");
        }
    }
}
