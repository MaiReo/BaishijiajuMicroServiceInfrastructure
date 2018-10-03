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
        /// <summary>
        /// All registered handler factories.
        /// Key: Type of the event
        /// Value: List of handler factories
        /// </summary>
        private readonly ConcurrentDictionary<Type, ICollection<IMessageHandlerFactory>> _handlerFactories;

        private readonly IServiceProvider _serviceProvider;

        private MessageBus()
        {
            _handlerFactories = new ConcurrentDictionary<Type, ICollection<IMessageHandlerFactory>>();
        }

        public MessageBus(IServiceProvider serviceProvider) : this()
        {
            _serviceProvider = serviceProvider;
        }

        /// <inheritdoc/>
        public virtual async Task PublishAsync<T>(T message) where T : IMessage
        {
            var messagePublisher = (IMessagePublisher)_serviceProvider.GetService(typeof(IMessagePublisher));
            if (messagePublisher == null)
            {
                return;
            }
            await messagePublisher.PublishAsync(message);
        }

        public virtual Task OnMessageReceivedAsync(IMessage message, IRichMessageDescriptor descriptor)
        {
            if (message is null)
            {
                return Task.CompletedTask;
            }
            return ProcessMessageAsync(message.GetType(), message, descriptor);
        }

        protected async Task ProcessMessageAsync(Type messageType, IMessage message, IRichMessageDescriptor descriptor)
        {
            var exceptions = new List<Exception>();

            await new SynchronizationContextRemover();

            foreach (var handlerFactories in GetHandlerFactories(messageType).ToList())
            {
                foreach (var handlerFactory in handlerFactories.MessageHandlerFactories.ToList())
                {
                    var handlerType = handlerFactory.GetHandlerType();

                    if (IsAsyncRichMessageHandler(handlerType))
                    {
                        await ProcessRichMessagecHandlingExceptionAsync(handlerFactory, handlerFactories.MessageType, message, descriptor, exceptions);
                    }
                    else if (IsAsyncMessageHandler(handlerType))
                    {
                        await ProcessMessagecHandlingExceptionAsync(handlerFactory, handlerFactories.MessageType, message, exceptions);
                    }
                    else if (IsRichMessageHandler(handlerType))
                    {
                        ProcessRichMessageHandlingException(handlerFactory, handlerFactories.MessageType, message, descriptor, exceptions);
                    }
                    else if (IsMessageHandler(handlerType))
                    {
                        ProcessMessageHandlingException(handlerFactory, handlerFactories.MessageType, message, exceptions);
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
                    await ProcessMessageAsync(baseMessageType, baseMessage, descriptor);
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


        private async Task ProcessRichMessagecHandlingExceptionAsync(IMessageHandlerFactory asyncHandlerFactory, Type messageType, IMessage message, IRichMessageDescriptor descriptor, List<Exception> exceptions)
        {
            var asyncEventHandler = asyncHandlerFactory.GetHandler();

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

                await (Task)method.Invoke(asyncEventHandler, new object[] { message, descriptor });
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

        private async Task ProcessMessagecHandlingExceptionAsync(IMessageHandlerFactory asyncHandlerFactory, Type messageType, IMessage message, List<Exception> exceptions)
        {
            var asyncEventHandler = asyncHandlerFactory.GetHandler();

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

        private void ProcessRichMessageHandlingException(IMessageHandlerFactory handlerFactory, Type messageType, IMessage message, IRichMessageDescriptor descriptor, List<Exception> exceptions)
        {
            var eventHandler = handlerFactory.GetHandler();
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
                handlerFactory.ReleaseHandler(eventHandler);
            }
        }

        private void ProcessMessageHandlingException(IMessageHandlerFactory handlerFactory, Type messageType, IMessage message, List<Exception> exceptions)
        {
            var eventHandler = handlerFactory.GetHandler();
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


        private ICollection<IMessageHandlerFactory> GetOrCreateHandlerFactories(Type messageType)
        {
            return _handlerFactories.GetOrAdd(messageType, (type) => new HashSet<IMessageHandlerFactory>(new MessageHandlerFactoryUniqueComparer()));
        }

        private IEnumerable<MessageTypeWithMessageHandlerFactories> GetHandlerFactories(Type messageType)
        {
            foreach (var handlerFactory in _handlerFactories.Where(hf => ShouldTriggerMessageForHandler(messageType, hf.Key)))
            {
                yield return new MessageTypeWithMessageHandlerFactories(handlerFactory.Key, handlerFactory.Value);
            }
        }

        private static bool ShouldTriggerMessageForHandler(Type messageType, Type registeredType)
        {
            //Should trigger same type
            if (registeredType == messageType)
            {
                return true;
            }

            //Should trigger for inherited types
            if (registeredType.IsAssignableFrom(messageType))
            {
                return true;
            }

            return false;
        }

        public IEnumerable<Type> GetAllHandledMessageTypes()
        {
            return _handlerFactories.Keys.ToList();
        }

        public IDisposable Register(Type messageType, IMessageHandlerFactory factory)
        {
            GetOrCreateHandlerFactories(messageType)
                .Locking(factories => factories.Add(factory));
            return new MessageHandlerFactoryUnregistrar(this, messageType, factory);
        }

        public void Unregister(Type messageType, IMessageHandlerFactory factory)
        {
            GetOrCreateHandlerFactories(messageType).Locking(factories => factories.Remove(factory));
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
