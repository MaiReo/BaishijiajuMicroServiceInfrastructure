using System.Collections.Generic;

namespace Core.Messages.Bus.Factories
{
    internal class MessageHandlerFactoryUniqueComparer : IEqualityComparer<IMessageHandlerFactory>
    {
        public bool Equals(IMessageHandlerFactory x, IMessageHandlerFactory y)
        {
            if (x == null && y == null) return true;
            if (x == null && y != null) return false;
            if (x != null && y == null) return false;
            return x.GetHandlerType() == y.GetHandlerType();
        }

        public int GetHashCode(IMessageHandlerFactory obj)
        {
            return obj?.GetHandlerType()?.GetHashCode() ?? 0;
        }
    }
}
