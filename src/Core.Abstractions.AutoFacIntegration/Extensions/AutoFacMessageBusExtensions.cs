using Autofac;
using Autofac.Core;
using Core.Messages.Bus.Factories;
using System;
using System.Linq;

namespace Core.Messages.Bus.Extensions
{

    public static class AutoFacMessageBusExtensions
    {
        private static readonly Type[] _messageHandlerTypes = new[]
       {
           typeof(IMessageHandler<>),
           typeof(IRichMessageHandler<>),
           typeof(IAsyncMessageHandler<>),
           typeof(IAsyncRichMessageHandler<>)
        };
        public static IMessageBus RegisterMessageHandlers(this IMessageBus messageBus, ILifetimeScope lifetimeScope)
        {
            foreach (var registration in lifetimeScope.ComponentRegistry.Registrations)
            {
                var handlerType = registration.Activator.LimitType;
                var services = registration.Services.OfType<IServiceWithType>().Select(x => x.ServiceType);
                foreach (var service in services)
                {
                    if (!typeof(IMessageHandler).IsAssignableFrom(service))
                    {
                        continue;
                    }
                    if (!service.IsGenericType)
                    {
                        continue;
                    }

                    var genDefType = service.GetGenericTypeDefinition();
                    if (!_messageHandlerTypes.Any(type => type == genDefType))
                    {
                        continue;
                    }
                    var messageType = service.GetGenericArguments().First();
                    messageBus.Register(messageType, new IocMessageHandlerFactory(handlerType));
                }
            }
            return messageBus;
        }
    }
}
