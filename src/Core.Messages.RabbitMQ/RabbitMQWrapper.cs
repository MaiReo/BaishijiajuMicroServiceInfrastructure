using Core.Messages.Bus;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Core.Messages
{
    public class RabbitMQWrapper : IRabbitMQWrapper
    {
        private readonly ConcurrentDictionary<IMessageDescriptor, IModel> _channels;
        private ConcurrentDictionary<IMessageDescriptor, Func<IMessage, IRichMessageDescriptor, Task>> _subscribers;
        private readonly IRabbitMQPersistentConnection _persistentConnection;
        private readonly IMessageBusOptions _messageBusOptions;
        private readonly IMessageConverter _messageConverter;

        public ILogger Logger { get; }

        protected RabbitMQWrapper()
        {
            _channels = new ConcurrentDictionary<IMessageDescriptor, IModel>(MessageDescriptorEqualityComparer.Instance);
            _subscribers = new ConcurrentDictionary<IMessageDescriptor, Func<IMessage, IRichMessageDescriptor, Task>>(MessageDescriptorEqualityComparer.Instance);
        }

        public RabbitMQWrapper(
            IRabbitMQPersistentConnection rabbitMQPersistentConnection,
            IMessageBusOptions messageBusOptions,
            IMessageConverter messageConverter,
            ILogger<RabbitMQWrapper> logger) : this()
        {
            _persistentConnection = rabbitMQPersistentConnection;
            _messageBusOptions = messageBusOptions;
            _messageConverter = messageConverter;
            Logger = (ILogger)logger ?? NullLogger.Instance;
        }

        public void Subscribe(IMessageDescriptor descriptor, Action<IMessage> handler) => Subscribe(descriptor, async message => await Task.Run(() => handler?.Invoke(message)));

        public void Subscribe(IMessageDescriptor descriptor, Action<IMessage, IRichMessageDescriptor> handler) => Subscribe(descriptor, async ( message, _descriptor) => await Task.Run(() => handler?.Invoke(message, _descriptor)));

        public void Subscribe(IMessageDescriptor descriptor, Func<IMessage, Task> asyncHandler) => Subscribe(descriptor, async (msg, desc) => await asyncHandler(msg));

        public void Subscribe(IMessageDescriptor descriptor, Func<IMessage, IRichMessageDescriptor, Task> asyncHandler)
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
            const string MODE = "topic";
            Logger.LogInformation($"CreateConsumerChannel: Exchange: {descriptor.MessageGroup}, RoutingKey: {descriptor.MessageTopic}, Mode:{MODE}");
            if (_channels.TryGetValue(descriptor, out var exists))
            {
                Logger.LogInformation($"CreateConsumerChannel: Channel is already created");
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
                                 type: MODE);

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
                var richDescriptor = new RichMessageDescriptor(ea.Exchange, ea.RoutingKey, ea.Redelivered, ea.BasicProperties?.ContentEncoding, ea.BasicProperties?.ContentType, ea.BasicProperties?.MessageId, ea.BasicProperties?.Persistent, ea.BasicProperties?.Headers);
                await ProcessMessageAsync(richDescriptor, ea.Body);
                channel.BasicAck(ea.DeliveryTag, multiple: false);
            };

            channel.BasicConsume(queue: _messageBusOptions.QueueName,
                                 autoAck: false,
                                 consumer: consumer);
            return channel;
        }

        private async Task ProcessMessageAsync(
            IRichMessageDescriptor descriptor, byte[] message)
        {
            Logger.LogInformation($"ProcessMessageAsync: Exchange: {descriptor.MessageGroup}, Topic: {descriptor.MessageTopic}");
            if (!_subscribers.TryGetValue(descriptor, out var func))
            {
                Logger.LogInformation($"ProcessMessageAsync: Subscriber not exists");
                return;
            }
            var typedMessage = _messageConverter.Deserialize(descriptor, message);
            if (typedMessage == null)
            {
                Logger.LogInformation($"ProcessMessageAsync: Cannot convert message to a typed message");
                return;
            }
            await func(typedMessage, descriptor);
        }
    }
}