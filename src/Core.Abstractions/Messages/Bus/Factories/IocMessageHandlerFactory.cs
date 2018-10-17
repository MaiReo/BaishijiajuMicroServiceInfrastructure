using System;

namespace Core.Messages.Bus.Factories
{
    public class IocMessageHandlerFactory : IMessageHandlerFactory
    {
        public Type HandlerType { get; }

        public IocMessageHandlerFactory(Type handlerType)
        {
            HandlerType = handlerType ?? throw new ArgumentNullException(nameof(handlerType));
        }

        public virtual IMessageHandler GetHandler(IMessageScope messageScope)
        {
            return messageScope.Resolve(HandlerType) as IMessageHandler;
        }

        public virtual Type GetHandlerType() => HandlerType;

        public virtual void ReleaseHandler(IMessageScope messageScope, IMessageHandler handler)
        {
            if (handler == null)
            {
                return;
            }
            messageScope.Release(handler);
        }
    }
}