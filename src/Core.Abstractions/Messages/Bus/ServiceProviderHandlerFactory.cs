using System;
using Core.Messages;
using Core.Messages.Factories;

namespace Core.Abstractions
{
    public class ServiceProviderHandlerFactory : IMessageHandlerFactory
    {
        public ServiceProviderHandlerFactory(Type handlerType)
        {
            HandlerType = handlerType;
        }

        public Type HandlerType { get; }

        public IMessageHandler GetHandler(IServiceProvider serviceProvider)
        {
            return serviceProvider.GetService(HandlerType) as IMessageHandler;
        }

        public Type GetHandlerType()
        {
            return HandlerType;
        }

        public void ReleaseHandler(IMessageHandler handler)
        {
            if (handler == null)
            {
                return;
            }

            if ((handler is IDisposable disposable))
            {
                disposable.Dispose();
            }
        }
    }
}