using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Core.Messages.Bus;
using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Core.Messages
{
    public class RabbitMQWrapper : IRabbitMQWrapper
    {
        private readonly ConcurrentDictionary<IMessageDescriptor, IModel> _channels;
        private ConcurrentDictionary<IMessageDescriptor, Func<IMessage, Task>> _subscribers;
        private readonly IRabbitMQPersistentConnection _persistentConnection;
        private readonly IMessageBusOptions _messageBusOptions;
        private readonly IMessageConverter _messageConverter;

        public ILogger Logger { get; }

        protected RabbitMQWrapper()
        {
            _channels = new ConcurrentDictionary<IMessageDescriptor, IModel>(MessageDescriptorEqualityComparer.Instance);
            _subscribers = new ConcurrentDictionary<IMessageDescriptor, Func<IMessage, Task>>(MessageDescriptorEqualityComparer.Instance);
        }

        public RabbitMQWrapper(
            IRabbitMQPersistentConnection rabbitMQPersistentConnection,
            IMessageBusOptions messageBusOptions,
            IMessageConverter messageConverter,
            ILogger<RabbitMQWrapper> logger) : this()
        {
            this._persistentConnection = rabbitMQPersistentConnection;
            this._messageBusOptions = messageBusOptions;
            this._messageConverter = messageConverter;
            Logger = (ILogger)logger ?? NullLogger.Instance;
        }

        public void Subscribe(IMessageDescriptor descriptor, Action<IMessage> handler)
        {
            async Task func(IMessage message) => await Task.Run(() => handler?.Invoke(message));
            Subscribe(descriptor, func);
        }

        public void Subscribe(IMessageDescriptor descriptor, Func<IMessage, Task> asyncHandler)
        {
            Logger.LogInformation($"Subscribing: Exchange: {descriptor.MessageGroup}, Topic: {descriptor.MessageTopic}");
            _subscribers.AddOrUpdate(descriptor, asyncHandler, (desc, oldValue) => asyncHandler);
            CreateConsumerChannel(descriptor);
        }

        public void UnSubscribe(IMessageDescriptor descriptor)
        {
            _subscribers.TryRemove(descriptor, out var value);
        }

        public void Publish(IMessageDescriptor descriptor, byte[] message)
        {
            if (!_persistentConnection.IsConnected)
            {
                _persistentConnection.TryConnect();
            }

            using (var channel = _persistentConnection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: descriptor.MessageGroup,
                                    durable: true,
                                    type: "topic");


                var properties = channel.CreateBasicProperties();
                properties.DeliveryMode = 2; // persistent

                channel.BasicPublish(exchange: descriptor.MessageGroup,
                                 routingKey: descriptor.MessageTopic,
                                 mandatory: true,
                                 basicProperties: properties,
                                 body: message);
            }
        }

        public Task PublishAsync(IMessageDescriptor descriptor, byte[] message)
        {
            return Task.Run(() => Publish(descriptor, message));
        }

        private IModel CreateConsumerChannel(IMessageDescriptor descriptor)
        {
            Logger.LogInformation($"CreateConsumerChannel: Exchange: {descriptor.MessageGroup}, Topic: {descriptor.MessageTopic}");
            if (_channels.TryGetValue(descriptor, out var exists))
            {
                Logger.LogInformation($"CreateConsumerChannel: channel already created");
                return exists;
            }

            if (!_persistentConnection.IsConnected)
            {
                Logger.LogInformation($"CreateConsumerChannel: Try connecting...");
                _persistentConnection.TryConnect();
            }

            var channel = _persistentConnection.CreateModel();

            channel.CallbackException += (sender, ea) =>
            {
                channel.Close();
                channel.Dispose();
                _channels.TryRemove(descriptor, out exists);
                channel = CreateConsumerChannel(descriptor);
                _channels.TryAdd(descriptor, channel);
            };

            channel.ExchangeDeclare(exchange: descriptor.MessageGroup,
                                 durable: true,
                                 type: "topic");

            channel.QueueDeclare(queue: _messageBusOptions.QueueName,
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            channel.QueueBind(queue: _messageBusOptions.QueueName,
                                  exchange: descriptor.MessageGroup,
                                  routingKey: descriptor.MessageTopic);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += async (model, ea) =>
            {
                var _descriptor = new MessageDescriptor(descriptor.MessageGroup, ea.RoutingKey);
                await ProcessMessageAsync(_descriptor, ea.Body);
                channel.BasicAck(ea.DeliveryTag, multiple: false);
            };

            channel.BasicConsume(queue: _messageBusOptions.QueueName,
                                 autoAck: false,
                                 consumer: consumer);
            return channel;
        }

        private async Task ProcessMessageAsync(IMessageDescriptor descriptor, byte[] message)
        {
            Logger.LogInformation($"ProcessMessageAsync: Exchange: {descriptor.MessageGroup}, Topic: {descriptor.MessageTopic}");
            if (!_subscribers.TryGetValue(descriptor, out var func))
            {
                Logger.LogInformation($"ProcessMessageAsync: Subscriber no exists");
                return;
            }
            var typedMessage = _messageConverter.Deserialize(descriptor, message);
            if (typedMessage == null)
            {
                Logger.LogInformation($"ProcessMessageAsync: Cannot convert message to a typed message");
                return;
            }
            await func(typedMessage);
        }
    }
}