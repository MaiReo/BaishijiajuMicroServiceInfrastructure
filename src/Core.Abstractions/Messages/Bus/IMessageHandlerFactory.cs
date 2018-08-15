using System;

namespace Core.Messages.Factories
{
    public interface IMessageHandlerFactory
    {
        IMessageHandler GetHandler();

        Type GetHandlerType();

        void ReleaseHandler(IMessageHandler handler);
    }
}