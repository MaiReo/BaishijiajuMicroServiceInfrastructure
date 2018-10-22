using Core.Messages.Bus;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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

        public void Subscribe(IMessageDescriptor descriptor, Action<IMessage, IRichMessageDescriptor> handler) => Subscribe(descriptor, async (message, _descriptor) => await Task.Run(() => handler?.Invoke(message, _descriptor)));

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

                if (descriptor is IRichMessageDescriptor rich && rich.Headers != null)
                {
                    var headers = properties.Headers = properties.Headers ?? new Dictionary<string, object>();
                    foreach (var item in rich.Headers)
                    {
                        headers.Add(item.Key, item.Value);
                    }
                }

                channel.BasicPublish(exchange: descriptor.MessageGroup,
                                 routingKey: descriptor.MessageTopic,
                                 mandatory: true,
                                 basicProperties: properties,
                                 body: message);
            }
        }

        public ValueTask PublishAsync(IMessageDescriptor descriptor, byte[] message)
        {
            return new ValueTask(Task.Run(() => Publish(descriptor, message)));
        }

        private IModel CreateConsumerChannel(IMessageDescriptor descriptor)
        {
            const string MODE = "topic";
            if (_channels.TryGetValue(descriptor, out var exists))
            {
                Logger.LogWarning($"CreateConsumerChannel: Channel is already created");
                return exists;
            }

            if (!_persistentConnection.IsConnected)
            {
                Logger.LogWarning($"CreateConsumerChannel: Try connecting...");
                _persistentConnection.TryConnect();
            }

            var channel = _persistentConnection.CreateModel();

            channel.CallbackException += (sender, ea) =>
            {
                if (!channel.IsClosed)
                {
                    channel.Close();
                }
                channel.Dispose();
                _channels.TryRemove(descriptor, out exists);
                channel = CreateConsumerChannel(descriptor);
                _channels.TryAdd(descriptor, channel);
            };

            channel.ModelShutdown += (sender, ea) =>
            {
                if (ea.Cause is null)
                {
                    return;
                }
                var _channel = (sender as IModel);
                if (_channel is null)
                {
                    return;
                }
                if (ea.ReplyCode == 541 && (_channel.IsOpen == false)) //IOException
                {
                    Logger.LogWarning($"CreateConsumerChannel: channel { _channel.ChannelNumber} binded to Exchange: {descriptor.MessageGroup}, RoutingKey: {descriptor.MessageTopic} shutdown caused by exception:{(ea.Cause as Exception)?.Message}, Try recreating...");
                    _channel.Dispose();
                    _channels.TryRemove(descriptor, out exists);
                    channel = CreateConsumerChannel(descriptor);
                    _channels.TryAdd(descriptor, channel);
                }
            };

            channel.ExchangeDeclare(exchange: descriptor.MessageGroup,
                                 durable: true,
                                 type: MODE);

            var queueName = _messageBusOptions.QueuePerConsumer != false ? string.Concat(_messageBusOptions.QueueName, "-", descriptor.MessageTopic) : _messageBusOptions.QueueName;

            channel.QueueDeclare(queue: queueName,
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            channel.QueueBind(queue: queueName,
                                  exchange: descriptor.MessageGroup,
                                  routingKey: descriptor.MessageTopic);


            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.Shutdown += async (sender, ea) =>
            {
                var _consumer = sender as AsyncDefaultBasicConsumer;
                Logger.LogWarning($"CreateConsumerChannel: Consumer with tag: {_consumer.ConsumerTag} shutdown");
                await Task.Yield();
            };

            consumer.Received += async (model, ea) =>
            {
                var richDescriptor = new RichMessageDescriptor(ea.Exchange, ea.RoutingKey, ea.Redelivered, ea.BasicProperties?.ContentEncoding, ea.BasicProperties?.ContentType, ea.BasicProperties?.MessageId, ea.BasicProperties?.Persistent, ea.BasicProperties?.Headers);
                await ProcessMessageAsync(richDescriptor, ea.Body);
                channel.BasicAck(ea.DeliveryTag, multiple: false);
                await Task.Yield();
            };

            var consumerTag = channel.BasicConsume(queue: _messageBusOptions.QueueName,
                                 autoAck: false,
                                 consumer: consumer);

            Logger.LogInformation($"CreateConsumerChannel: Exchange: {descriptor.MessageGroup}, RoutingKey: {descriptor.MessageTopic}, Mode:{MODE}, Queue: {queueName}, ConsumerTag: {consumerTag}");

            return channel;
        }

        private async ValueTask ProcessMessageAsync(
            IRichMessageDescriptor descriptor, byte[] message)
        {
            Logger.LogInformation($"ProcessMessageAsync: Exchange: {descriptor.MessageGroup}, Topic: {descriptor.MessageTopic}");
            if (!_subscribers.TryGetValue(descriptor, out var func))
            {
                Logger.LogWarning($"ProcessMessageAsync: Subscriber not exists");
                return;
            }
            var typedMessage = _messageConverter.Deserialize(descriptor, message);
            if (typedMessage == null)
            {
                Logger.LogWarning($"ProcessMessageAsync: Cannot convert message to a typed message");
                return;
            }
            await func(typedMessage, descriptor);
            await Task.Yield();
        }
    }
}