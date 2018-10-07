using Abp.Dependency;
using System;

namespace Core.Messages.Bus
{
    internal class MessageScope : IMessageScope
    {
        private readonly IScopedIocResolver _iocResolver;

        public MessageScope(IIocResolver iocResolver)
        {
            _iocResolver = iocResolver.CreateScope();
        }

        public void Dispose()
        {
            _iocResolver.Dispose();
        }

        public void Release(IMessageHandler handler)
        {
            _iocResolver.Release(handler);
        }

        public object Resolve(Type type)
        {
            return _iocResolver.Resolve(type);
        }
    }
}
