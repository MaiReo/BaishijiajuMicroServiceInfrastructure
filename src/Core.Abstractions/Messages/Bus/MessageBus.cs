using Core.Messages.Bus.Factories;
using Core.Messages.Store;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Messages.Bus
{
    public class MessageBus : IMessageBus
    {

        private readonly IMessageHandlerFactoryStore _messageHandlerFactoryStore;
        private readonly IMessagePublisher _messagePublisher;
        private readonly IMessageScopeCreator _scopeCreator;
        private readonly ILogger _logger;

        public MessageBus(
            IMessageHandlerFactoryStore messageHandlerFactoryStore,
            IMessagePublisher messagePublisher,
            IMessageScopeCreator scopeCreator,
            ILogger<MessageBus> logger = null)
        {
            _messageHandlerFactoryStore = messageHandlerFactoryStore;
            _messagePublisher = messagePublisher;
            _scopeCreator = scopeCreator;
            _logger = (ILogger)logger ?? NullLogger.Instance; ;
        }

        /// <inheritdoc/>
        public virtual async ValueTask PublishAsync<T>(T message) where T : IMessage
        {
            await _messagePublisher.PublishAsync(message);
        }

        public virtual async ValueTask OnMessageReceivedAsync(IMessage message, IRichMessageDescriptor descriptor)
        {
            if (message is null)
            {
                return;
            }
            using (var scope = _scopeCreator.CreateScope(message, descriptor))
            {
                var messageStore = scope.Resolve(typeof(IConsumedMessageStore)) as IConsumedMessageStore;

                if (await messageStore.IsConsumedAsync(descriptor, message))
                {
                    _logger.LogWarning($"[{descriptor.MessageGroup}][{descriptor.MessageTopic}][{descriptor.MessageId}]Message already consumed.");
                    return;
                }
                await ProcessMessageAsync(scope, message.GetType(), message, descriptor);
                await messageStore.StoreAsync(descriptor, message);
            }
        }

        protected async ValueTask ProcessMessageAsync(IMessageScope scope, Type messageType, IMessage message, IRichMessageDescriptor descriptor)
        {
            var exceptions = new List<Exception>();

            await new SynchronizationContextRemover();

            foreach (var handlerFactories in _messageHandlerFactoryStore.GetHandlerFactories(messageType).ToList())
            {
                foreach (var handlerFactory in handlerFactories.MessageHandlerFactories)
                {
                    var handlerType = handlerFactory.GetHandlerType();

                    if (IsAsyncRichMessageHandler(handlerType))
                    {
                        await ProcessRichMessagecHandlingExceptionAsync(scope, handlerFactory, handlerFactories.MessageType, message, descriptor, exceptions);
                    }
                    else if (IsAsyncMessageHandler(handlerType))
                    {
                        await ProcessMessagecHandlingExceptionAsync(scope, handlerFactory, handlerFactories.MessageType, message, exceptions);
                    }
                    else if (IsRichMessageHandler(handlerType))
                    {
                        ProcessRichMessageHandlingException(scope, handlerFactory, handlerFactories.MessageType, message, descriptor, exceptions);
                    }
                    else if (IsMessageHandler(handlerType))
                    {
                        ProcessMessageHandlingException(scope, handlerFactory, handlerFactories.MessageType, message, exceptions);
                    }
                    else
                    {
                        var errorMessage = $"Event handler to register for event type {messageType.Name} does not implement IMessageHandler<{messageType.Name}> or IMessageEventHandler<{messageType.Name}> interface!";
                        exceptions.Add(new MessageBusException(errorMessage));
                    }
                }
            }

            //Implements generic argument inheritance. See IMessageWithInheritableGenericArgument
            if (messageType.IsGenericType &&
                messageType.GenericTypeArguments.Length == 1 &&
                typeof(IMessageWithInheritableGenericArgument).IsAssignableFrom(messageType))
            {
                var genericArg = messageType.GetGenericArguments()[0];
                var baseArg = genericArg.BaseType;
                if (baseArg != null)
                {
                    var baseMessageType = messageType.GetGenericTypeDefinition().MakeGenericType(baseArg);
                    var constructorArgs = ((IMessageWithInheritableGenericArgument)messageType).GetConstructorArgs();
                    var baseMessage = (IMessage)Activator.CreateInstance(baseMessageType, constructorArgs);
                    await ProcessMessageAsync(scope, baseMessageType, baseMessage, descriptor);
                }
            }

            if (exceptions.Any())
            {
                if (exceptions.Count == 1)
                {
                    ExceptionDispatchInfo.Capture(exceptions[0]).Throw();
                }

                throw new AggregateException("More than one error has occurred while handling the message: " + messageType, exceptions);
            }
        }


        private async ValueTask ProcessRichMessagecHandlingExceptionAsync(IMessageScope scope, IMessageHandlerFactory asyncHandlerFactory, Type messageType, IMessage message, IRichMessageDescriptor descriptor, List<Exception> exceptions)
        {
            var asyncEventHandler = asyncHandlerFactory.GetHandler(scope);

            try
            {
                if (asyncEventHandler == null)
                {
                    throw new ArgumentNullException($"Registered async rich message handler for event type {messageType.Name} is null!");
                }

                var asyncHandlerType = typeof(IAsyncRichMessageHandler<>).MakeGenericType(messageType);

                var method = asyncHandlerType.GetMethod(
                    nameof(IAsyncRichMessageHandler<IMessage>.HandleMessageAsync),
                    new[] { messageType, typeof(IRichMessageDescriptor) }
                );

                await (ValueTask)method.Invoke(asyncEventHandler, new object[] { message, descriptor });
            }
            catch (TargetInvocationException ex)
            {
                exceptions.Add(ex.InnerException);
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }
            finally
            {
                asyncHandlerFactory.ReleaseHandler(scope, asyncEventHandler);
            }
        }

        private async ValueTask ProcessMessagecHandlingExceptionAsync(IMessageScope scope, IMessageHandlerFactory asyncHandlerFactory, Type messageType, IMessage message, List<Exception> exceptions)
        {
            var asyncEventHandler = asyncHandlerFactory.GetHandler(scope);

            try
            {
                if (asyncEventHandler == null)
                {
                    throw new ArgumentNullException($"Registered async message handler for event type {messageType.Name} is null!");
                }

                var asyncHandlerType = typeof(IAsyncMessageHandler<>).MakeGenericType(messageType);

                var method = asyncHandlerType.GetMethod(
                    nameof(IAsyncMessageHandler<IMessage>.HandleMessageAsync),
                    new[] { messageType }
                );

                await (ValueTask)method.Invoke(asyncEventHandler, new object[] { message });
            }
            catch (TargetInvocationException ex)
            {
                exceptions.Add(ex.InnerException);
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }
            finally
            {
                asyncHandlerFactory.ReleaseHandler(scope, asyncEventHandler);
            }
        }

        private void ProcessRichMessageHandlingException(IMessageScope scope, IMessageHandlerFactory handlerFactory, Type messageType, IMessage message, IRichMessageDescriptor descriptor, List<Exception> exceptions)
        {
            var eventHandler = handlerFactory.GetHandler(scope);
            try
            {
                if (eventHandler == null)
                {
                    throw new ArgumentNullException($"Registered rich message handler for event type {messageType.Name} is null!");
                }

                var handlerType = typeof(IRichMessageHandler<>).MakeGenericType(messageType);

                var method = handlerType.GetMethod(
                    nameof(IRichMessageHandler<IMessage>.HandleMessage),
                    new[] { messageType, typeof(IRichMessageDescriptor) }
                );

                method.Invoke(eventHandler, new object[] { message, descriptor });
            }
            catch (TargetInvocationException ex)
            {
                exceptions.Add(ex.InnerException);
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }
            finally
            {
                handlerFactory.ReleaseHandler(scope, eventHandler);
            }
        }

        private void ProcessMessageHandlingException(IMessageScope scope, IMessageHandlerFactory handlerFactory, Type messageType, IMessage message, List<Exception> exceptions)
        {
            var eventHandler = handlerFactory.GetHandler(scope);
            try
            {
                if (eventHandler == null)
                {
                    throw new ArgumentNullException($"Registered message handler for event type {messageType.Name} is null!");
                }

                var handlerType = typeof(IMessageHandler<>).MakeGenericType(messageType);

                var method = handlerType.GetMethod(
                    nameof(IMessageHandler<IMessage>.HandleMessage),
                    new[] { messageType }
                );

                method.Invoke(eventHandler, new object[] { message });
            }
            catch (TargetInvocationException ex)
            {
                exceptions.Add(ex.InnerException);
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }
            finally
            {
                handlerFactory.ReleaseHandler(scope, eventHandler);
            }
        }




        private bool IsMessageHandler(Type handlerType)
        {
            return handlerType.GetInterfaces()
                .Where(i => i.IsGenericType)
                .Any(i => i.GetGenericTypeDefinition() == typeof(IMessageHandler<>));
        }

        private bool IsRichMessageHandler(Type handlerType)
        {
            return handlerType.GetInterfaces()
                .Where(i => i.IsGenericType)
                .Any(i => i.GetGenericTypeDefinition() == typeof(IRichMessageHandler<>));
        }

        private bool IsAsyncMessageHandler(Type handlerType)
        {
            return handlerType.GetInterfaces()
                .Where(i => i.IsGenericType)
                .Any(i => i.GetGenericTypeDefinition() == typeof(IAsyncMessageHandler<>));
        }

        private bool IsAsyncRichMessageHandler(Type handlerType)
        {
            return handlerType.GetInterfaces()
                .Where(i => i.IsGenericType)
                .Any(i => i.GetGenericTypeDefinition() == typeof(IAsyncRichMessageHandler<>));
        }

        public IEnumerable<Type> GetAllHandledMessageTypes()
        {
            return _messageHandlerFactoryStore.GetAllHandledMessageTypes();
        }

        public IDisposable Register(Type messageType, IMessageHandlerFactory factory)
        {
            return _messageHandlerFactoryStore.Register(messageType, factory);
        }

        public void Unregister(Type messageType, IMessageHandlerFactory factory)
        {
            _messageHandlerFactoryStore.Unregister(messageType, factory);
        }

        // Reference from
        // https://github.com/aspnetboilerplate/aspnetboilerplate/blob/dev/src/Abp/Events/Bus/EventBus.cs
        // https://blogs.msdn.microsoft.com/benwilli/2017/02/09/an-alternative-to-configureawaitfalse-everywhere/
        private struct SynchronizationContextRemover : INotifyCompletion
        {
            public bool IsCompleted
            {
                get { return SynchronizationContext.Current == null; }
            }

            public void OnCompleted(Action continuation)
            {
                var prevContext = SynchronizationContext.Current;
                try
                {
                    SynchronizationContext.SetSynchronizationContext(null);
                    continuation();
                }
                finally
                {
                    SynchronizationContext.SetSynchronizationContext(prevContext);
                }
            }

            public SynchronizationContextRemover GetAwaiter()
            {
                return this;
            }

            public void GetResult()
            {
            }
        }


    }
}
