using System;

namespace Core.Messages.Factories
{
    public interface IMessageHandlerFactory
    {
        IMessageHandler GetHandler(IServiceProvider serviceProvider);

        Type GetHandlerType();

        void ReleaseHandler(IMessageHandler handler);
    }
}