using Core.Messages;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Abstractions.Tests
{
    internal class FakeMessagePublisherWrapper : IMessagePublisherWrapper
    {
        public FakeMessagePublisherWrapper()
        {
            PublishParameters = new List<IMessageWrapper>();
            PublishAsyncParameters = new List<IMessageWrapper>();
        }
        public List<IMessageWrapper> PublishParameters { get; }

        public List<IMessageWrapper> PublishAsyncParameters { get; }

        public void Publish(IMessageWrapper messageWrapper)
        {
            PublishParameters.Add(messageWrapper);
        }

        public ValueTask PublishAsync(IMessageWrapper messageWrapper)
        {
            PublishAsyncParameters.Add(messageWrapper);
            return new ValueTask();
        }

        public static FakeMessagePublisherWrapper Instance => new FakeMessagePublisherWrapper();
    }
}