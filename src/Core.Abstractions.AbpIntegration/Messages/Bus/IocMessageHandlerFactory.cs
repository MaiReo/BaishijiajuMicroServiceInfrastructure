using Abp.Dependency;
using System;

namespace Core.Messages.Factories
{
    public class IocMessageHandlerFactory : IMessageHandlerFactory
    {
        private readonly IIocResolver _iocResolver;

        public Type HandlerType { get; }

        public IocMessageHandlerFactory(IIocResolver iocResolver, Type handlerType)
        {
            this._iocResolver = iocResolver ?? throw new ArgumentNullException(nameof(iocResolver));
            HandlerType = handlerType ?? throw new ArgumentNullException(nameof(handlerType));
        }

        public virtual IMessageHandler GetHandler(IServiceProvider serviceProvider)
        {
            return this._iocResolver.Resolve(HandlerType) as IMessageHandler;
        }

        public virtual Type GetHandlerType() => HandlerType;

        public virtual void ReleaseHandler(IMessageHandler handler)
        {
            if (handler == null)
            {
                return;
            }
            this._iocResolver.Release(handler);
        }
    }
}