using Core.Messages.Bus.Internal;
using System;

namespace Core.Messages.Bus.Factories
{
    public interface IMessageHandlerFactory
    {
        /// <summary>
        ///  Must call <see cref="ReleaseHandler"/> after use.
        /// </summary>
        /// <param name="scope"></param>
        /// <returns></returns>
        IMessageHandler GetHandler(IMessageScope scope);

        [Obsolete("Use GetHandlerDescriptor instead")]
        Type GetHandlerType();

        MessageHandlerDescriptor GetHandlerDescriptor();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="messageScope"></param>
        /// <param name="handler"></param>
        void ReleaseHandler(IMessageScope messageScope, IMessageHandler handler);
    }
}