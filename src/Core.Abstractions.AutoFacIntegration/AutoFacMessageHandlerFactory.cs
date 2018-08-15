using Autofac;
using System;

namespace Core.Messages.Factories
{
    internal class AutoFacMessageHandlerFactory : IMessageHandlerFactory
    {
        private readonly IComponentContext _componentContext;
        private readonly Type _handlerType;

        public AutoFacMessageHandlerFactory(IComponentContext componentContext, Type handlerType)
        {
            this._componentContext = componentContext;
            this._handlerType = handlerType;
        }

        public IMessageHandler GetHandler() => (IMessageHandler)this._componentContext.Resolve(_handlerType);

        public Type GetHandlerType() => _handlerType;

        public void ReleaseHandler(IMessageHandler handler)
        {

        }
    }
}
