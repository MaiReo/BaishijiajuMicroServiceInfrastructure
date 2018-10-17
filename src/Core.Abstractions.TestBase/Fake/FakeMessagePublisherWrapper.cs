using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Messages.Fake
{
    public class FakeMessagePublisherWrapper : IMessagePublisherWrapper
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

        public Task PublishAsync(IMessageWrapper messageWrapper)
        {
            PublishAsyncParameters.Add(messageWrapper);
            return Task.CompletedTask;
        }
    }
}
