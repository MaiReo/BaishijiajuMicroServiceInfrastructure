using System;

namespace Core.Messages.Bus.Factories
{
    public interface IMessageHandlerFactory
    {
        IMessageHandler GetHandler(IMessageScope scope);

        Type GetHandlerType();

        void ReleaseHandler(IMessageScope messageScope, IMessageHandler handler);
    }
}