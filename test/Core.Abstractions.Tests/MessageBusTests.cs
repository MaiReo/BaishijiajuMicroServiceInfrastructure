using Core.Abstractions.Dependency;
using Core.Messages;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

    public class TestRichMessageHandler : IRichMessageHandler<TestMessage>, ILifestyleSingleton
    {
        public TestRichMessageHandler()
        {
            _parameters = new List<(TestMessage, IMessageDescriptor)>();
        }
        public List<(TestMessage, IMessageDescriptor)> _parameters;
        public IReadOnlyCollection<(TestMessage, IMessageDescriptor)> Parameters => _parameters;
        public void HandleMessage(TestMessage message, IRichMessageDescriptor descriptor)
        {
            _parameters.Add((message, descriptor));
        }
    }


    public class MessageBusTests : TestBase.AbstractionTestBase<MessageBusTests>
    {

        [Fact(DisplayName = "消息总线处理消息")]
        public async ValueTask Should_Trigger()
        {
            await MessageBus.OnMessageReceivedAsync(new TestMessage { Name = "testmsg" }, null);
            Resolve<TestMessageHandler>().Parameters.Single(x => x.Name == "testmsg");
        }

        [Fact(DisplayName = "消息总线处理富消息")]
        public async ValueTask Should_Trigger_Rich()
        {
            await MessageBus.OnMessageReceivedAsync(new TestMessage { Name = "testmsg" }, new RichMessageDescriptor("test.group", "test.topic"));
            Resolve<TestRichMessageHandler>().Parameters.Single(x => x.Item1.Name == "testmsg" && x.Item2.MessageTopic == "test.topic");
        }
    }
}
