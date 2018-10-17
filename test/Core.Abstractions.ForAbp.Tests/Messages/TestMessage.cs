using Core.Messages;

namespace Core.Abstractions.Tests.Messages
{
    public class TestMessage : IMessage
    {
        public string TestTitle { get; set; }
    }
}
