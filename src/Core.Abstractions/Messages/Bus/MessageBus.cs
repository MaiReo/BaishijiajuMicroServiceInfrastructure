using Core.Abstractions;
using Core.Messages.Factories;
using System;
using System.Collections.Concurrent;
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

        private readonly IServiceProvider _serviceProvider;
        private readonly IMessageHandlerFactoryStore _messageHandlerFactoryStore;

        public MessageBus(IServiceProvider serviceProvider,
            IMessageHandlerFactoryStore messageHandlerFactoryStore)
        {
            this._serviceProvider = serviceProvider;
            this._messageHandlerFactoryStore = messageHandlerFactoryStore;
        }

        /// <inheritdoc/>
        public virtual async Task PublishAsync<T>(T message) where T : IMessage
        {
            if (!(this._serviceProvider.GetService(typeof(IMessagePublisher)) is IMessagePublisher messagePublisher))
            {
                return;
            }
            await messagePublisher.PublishAsync(message);
        }

        /// <inheritdoc/>
        public virtual void OnMessageReceived(IMessage message)
        {
            if (message == null)
            {
                return;
            }
            OnMessageReceived(message.GetType(), message);
        }

        public virtual Task OnMessageReceivedAsync(IMessage message)
        {
            if (message == null)
            {
                return Task.CompletedTask;
            }
            return OnMessageReceivedAsync(message.GetType(), message);
        }

        protected virtual void OnMessageReceived(Type messageType, IMessage message)
        {
            var exceptions = new List<Exception>();

            foreach (var handlerFactories in GetHandlerFactories(messageType))
            {
                foreach (var handlerFactory in handlerFactories.MessageHandlerFactories)
                {
                    var handlerType = handlerFactory.GetHandlerType();

                    if (IsAsyncMessageHandler(handlerType))
                    {
                        OnMessageReceivedAsyncHandlingException(handlerFactory, handlerFactories.MessageType, message, exceptions)
                            .GetAwaiter()
                            .GetResult();
                    }
                    else if (IsMessageHandler(handlerType))
                    {
                        OnMessageReceivedHandlingException(handlerFactory, handlerFactories.MessageType, message, exceptions);
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
                    OnMessageReceived(baseMessageType, baseMessage);
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

        protected virtual async Task OnMessageReceivedAsync(Type messageType, IMessage message)
        {
            var exceptions = new List<Exception>();

            await new SynchronizationContextRemover();

            foreach (var handlerFactories in GetHandlerFactories(messageType))
            {
                foreach (var handlerFactory in handlerFactories.MessageHandlerFactories)
                {
                    var handlerType = handlerFactory.GetHandlerType();

                    if (IsAsyncMessageHandler(handlerType))
                    {
                        await OnMessageReceivedAsyncHandlingException(handlerFactory, handlerFactories.MessageType, message, exceptions);
                    }
                    else if (IsMessageHandler(handlerType))
                    {
                        OnMessageReceivedHandlingException(handlerFactory, handlerFactories.MessageType, message, exceptions);
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
                    await OnMessageReceivedAsync(baseMessageType, baseMessage);
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

        private void OnMessageReceivedHandlingException(IMessageHandlerFactory handlerFactory, Type messageType, IMessage message, List<Exception> exceptions)
        {
            var eventHandler = handlerFactory.GetHandler(this._serviceProvider);
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
                handlerFactory.ReleaseHandler(eventHandler);
            }
        }

        private async Task OnMessageReceivedAsyncHandlingException(IMessageHandlerFactory asyncHandlerFactory, Type messageType, IMessage message, List<Exception> exceptions)
        {
            var asyncEventHandler = asyncHandlerFactory.GetHandler(this._serviceProvider);

            try
            {
                if (asyncEventHandler == null)
                {
                    throw new ArgumentNullException($"Registered async event handler for event type {messageType.Name} is null!");
                }

                var asyncHandlerType = typeof(IAsyncMessageHandler<>).MakeGenericType(messageType);

                var method = asyncHandlerType.GetMethod(
                    nameof(IAsyncMessageHandler<IMessage>.HandleMessageAsync),
                    new[] { messageType }
                );

                await (Task)method.Invoke(asyncEventHandler, new object[] { message });
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
                asyncHandlerFactory.ReleaseHandler(asyncEventHandler);
            }
        }



        private bool IsMessageHandler(Type handlerType)
        {
            return handlerType.GetInterfaces()
                .Where(i => i.IsGenericType)
                .Any(i => i.GetGenericTypeDefinition() == typeof(IMessageHandler<>));
        }

        private bool IsAsyncMessageHandler(Type handlerType)
        {
            return handlerType.GetInterfaces()
                .Where(i => i.IsGenericType)
                .Any(i => i.GetGenericTypeDefinition() == typeof(IAsyncMessageHandler<>));
        }

        private IEnumerable<MessageTypeWithMessageHandlerFactories> GetHandlerFactories(Type eventType)
        {
            foreach (var handlerFactory in this._messageHandlerFactoryStore.GetHandlerFactories().Where(hf => ShouldTriggerMessageForHandler(eventType, hf.Key)))
            {
                yield return new MessageTypeWithMessageHandlerFactories(handlerFactory.Key, handlerFactory.Value);
            }
        }

        private static bool ShouldTriggerMessageForHandler(Type messageType, Type handlerType)
        {
            //Should trigger same type
            if (handlerType == messageType)
            {
                return true;
            }

            //Should trigger for inherited types
            if (handlerType.IsAssignableFrom(messageType))
            {
                return true;
            }

            return false;
        }

        public IEnumerable<Type> GetAllHandledMessageTypes()
        {
            return this._messageHandlerFactoryStore.GetAllHandledMessageTypes();
        }

        public IDisposable Register(Type messageType, IMessageHandlerFactory factory)
        {
            return this._messageHandlerFactoryStore.Register(messageType, factory);
        }

        public void Unregister(Type messageType, IMessageHandlerFactory factory)
        {
            this._messageHandlerFactoryStore.Unregister(messageType, factory);
        }

        private class MessageTypeWithMessageHandlerFactories
        {
            public Type MessageType { get; }

            public IEnumerable<IMessageHandlerFactory> MessageHandlerFactories { get; }

            public MessageTypeWithMessageHandlerFactories(Type messageType, IEnumerable<IMessageHandlerFactory> eventHandlerFactories)
            {
                MessageType = messageType;
                MessageHandlerFactories = eventHandlerFactories;
            }
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
