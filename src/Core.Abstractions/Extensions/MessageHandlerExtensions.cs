using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Messages.Bus.Internal
{
    public static class MessageHandlerExtensions
    {

        /// <summary>
        /// Do not use this api in your code directly. This api may be change or removed.
        /// </summary>
        /// <param name="handlerType"></param>
        /// <returns></returns>
        public static IEnumerable<MessageHandlerDescriptor> GetMessageHandlerDescriptors(this Type type)
        {
            if (type.IsAbstract)
            {
                yield break;
            }
            if (type.IsInterface)
            {
                yield break;
            }

            foreach (var descriptor in GetSyncMessageHandlerDescriptors(type))
            {
                yield return descriptor;
            }

            foreach (var descriptor in GetSyncRichMessageHandlerDescriptors(type))
            {
                yield return descriptor;
            }

            foreach (var descriptor in GetAsyncMessageHandlerDescriptors(type))
            {
                yield return descriptor;
            }

            foreach (var descriptor in GetAsyncRichMessageHandlerDescriptors(type))
            {
                yield return descriptor;
            }

        }

        private static IEnumerable<MessageHandlerDescriptor> GetSyncMessageHandlerDescriptors(Type type) => type.GetInterfaces()
                .Where(i => i.IsGenericType)
                .Where(i => i.GetGenericTypeDefinition() == typeof(IMessageHandler<>))
                .Select(i => new MessageHandlerDescriptor(type, i.GetGenericArguments()[0]));

        private static IEnumerable<MessageHandlerDescriptor> GetSyncRichMessageHandlerDescriptors(Type type) => type.GetInterfaces()
                .Where(i => i.IsGenericType)
                .Where(i => i.GetGenericTypeDefinition() == typeof(IRichMessageHandler<>))
                .Select(i => new MessageHandlerDescriptor(type, i.GetGenericArguments()[0], isRich: true));


        private static IEnumerable<MessageHandlerDescriptor> GetAsyncMessageHandlerDescriptors(Type type) => type.GetInterfaces()
                .Where(i => i.IsGenericType)
                .Where(i => i.GetGenericTypeDefinition() == typeof(IAsyncMessageHandler<>))
                .Select(i => new MessageHandlerDescriptor(type, i.GetGenericArguments()[0], isAsync: true));


        private static IEnumerable<MessageHandlerDescriptor> GetAsyncRichMessageHandlerDescriptors(Type type) => type.GetInterfaces()
                .Where(i => i.IsGenericType)
                .Where(i => i.GetGenericTypeDefinition() == typeof(IAsyncRichMessageHandler<>))
                .Select(i => new MessageHandlerDescriptor(type, i.GetGenericArguments()[0], true, true));
    }
}
