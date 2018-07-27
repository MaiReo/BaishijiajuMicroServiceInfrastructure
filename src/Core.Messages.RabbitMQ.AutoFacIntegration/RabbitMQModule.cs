using Autofac;
using Core.Messages.Bus;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;

namespace Core.Messages
{
    public class RabbitMQModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterIfNot<IMessagePublisherWrapper, RabbitMQMessagePublisherWrapper>(ServiceLifetime.Transient);
            builder.RegisterIfNot<IMessageSubscriber, MessageSubscriber>(ServiceLifetime.Singleton);
            builder.RegisterIfNot<IRabbitMQPersistentConnection, RabbitMQPersistentConnection>(ServiceLifetime.Singleton);
            builder.RegisterIfNot<IRabbitMQWrapper, RabbitMQWrapper>(ServiceLifetime.Singleton);

            builder.Register((cxt) =>
            {
                var options = cxt.Resolve<IMessageBusOptions>();
                var factory = new ConnectionFactory
                {
                    UserName = options.UserName,
                    Password = options.Password,
                    VirtualHost = options.VirtualHost,
                    HostName = options.HostName
                };
                return factory;
            })
            .AsSelf()
            .As<IConnectionFactory>()
            .SingleInstance()
            .ExternallyOwned();
        }
    }
}
