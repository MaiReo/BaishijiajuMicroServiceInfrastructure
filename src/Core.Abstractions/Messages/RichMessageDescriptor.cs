﻿using System.Collections.Generic;

namespace Core.Messages
{
    public class RichMessageDescriptor : MessageDescriptor, IRichMessageDescriptor, IMessageDescriptor
    {
        

        public RichMessageDescriptor(string messageGroup, string messageTopic) : base(messageGroup, messageTopic)
        {
            
        }

        public RichMessageDescriptor(string messageGroup, string messageTopic, bool redelivered, string contentEncoding, string contentType, string messageId, bool? persistent, IDictionary<string, object> headers) : this(messageGroup, messageTopic)
        {
            Redelivered = redelivered;
            ContentEncoding = contentEncoding;
            ContentType = contentType;
            MessageId = messageId;
            Persistent = persistent;
            Headers = headers;
        }

        /// <summary>
        /// 是否是第二次接收
        /// </summary>
        public bool Redelivered { get;}

        public string ContentEncoding { get; }

        public string ContentType { get; }

        public string MessageId { get;}

        public bool? Persistent { get; }

        public IDictionary<string, object> Headers { get; }

    }
}