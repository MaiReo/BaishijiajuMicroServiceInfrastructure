using System;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Messages.Utilities
{
    public class MessageHasher : IMessageHasher, IDisposable
    {
        private readonly ConcurrentDictionary<HashAlgorithmName, HashAlgorithm> _hasherCache;
        private readonly IMessageConverter _messageConverter;

        protected MessageHasher()
        {
            _hasherCache = new ConcurrentDictionary<HashAlgorithmName, HashAlgorithm>();
        }

        public MessageHasher(IMessageConverter messageConverter) : this()
        {
            _messageConverter = messageConverter;
        }

        public async ValueTask<string> HashAsync(
            IMessageDescriptor descriptor,
            IMessage message,
            HashAlgorithmName algorism = default,
            CancellationToken cancellationToken = default)
        {
            algorism = (algorism == default) ? HashAlgorithmName.SHA1 : algorism;
            //On Consuming
            if (descriptor is IRichMessageDescriptor rich && (!string.IsNullOrWhiteSpace(rich.MessageId)))
            {
                return await Task.Run(async () =>
                {
                    var messageId = Encoding.UTF8.GetBytes(rich.MessageId);
                    return await HashBodyAsync(descriptor, messageId, algorism, cancellationToken);
                });
            }
            //On Publishing
            return await Task.Run(async () =>
            {
                var messageBody = _messageConverter.Serialize(message);
                return await HashBodyAsync(descriptor, messageBody, algorism, cancellationToken);
            });
        }

        public async ValueTask<string> HashBodyAsync(
            IMessageDescriptor descriptor,
            byte[] messageBody,
            HashAlgorithmName algorism = default,
            CancellationToken cancellationToken = default)
        {
            algorism = (algorism == default) ? HashAlgorithmName.SHA1 : algorism;
            return await Task.Run(() =>
            {
                var hasher = _hasherCache.GetOrAdd(algorism, (name) => HashAlgorithm.Create(name.Name));
                {
                    var hash = hasher.ComputeHash(messageBody);
                    var hashString = Encoding.UTF8.GetString(hash);
                    return hashString;
                }
            });
        }

        #region IDisposable Support
        private bool disposedValue = false; // 要检测冗余调用



        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)。
                    foreach (var item in _hasherCache.Values)
                    {
                        item.Dispose();
                    }
                    _hasherCache.Clear();
                }

                // TODO: 释放未托管的资源(未托管的对象)并在以下内容中替代终结器。
                // TODO: 将大型字段设置为 null。

                disposedValue = true;
            }
        }

        // TODO: 仅当以上 Dispose(bool disposing) 拥有用于释放未托管资源的代码时才替代终结器。
        // ~MessageHasher() {
        //   // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
        //   Dispose(false);
        // }

        // 添加此代码以正确实现可处置模式。
        public void Dispose()
        {
            // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
            Dispose(true);
            // TODO: 如果在以上内容中替代了终结器，则取消注释以下行。
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
