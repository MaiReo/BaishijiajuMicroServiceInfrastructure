using System;
using System.Collections.Concurrent;
using System.Linq;
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

            byte[] raw = null;

            if (descriptor is IRichMessageDescriptor rich)
            {
                if (rich.Raw != null && rich.Raw.Length != 0)
                {
                    raw = rich.Raw;
                }
                else if (!string.IsNullOrWhiteSpace(rich.MessageId))
                {
                    raw = Encoding.UTF8.GetBytes(rich.MessageId);
                }
            }
            if (raw == null)
            {
                try
                {
                    raw = _messageConverter.Serialize(message);
                }
                catch (Exception)
                {
                }
            }
            if (raw == null)
            {
                return "SRC-ERROR";
            }
            return await Task.Run(async () => await HashBodyAsync(descriptor, raw, algorism, cancellationToken));
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
                try
                {
                    var hasher = _hasherCache.GetOrAdd(algorism, (name) => HashAlgorithm.Create(name.Name));
                    var hash = hasher.ComputeHash(messageBody);
                    var hashString = string.Join("", hash.Select(b => b.ToString("x2")));
                    return hashString;
                }
                catch (Exception e)
                {
                    return $"HASH-ERROR:{e.Message}";
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
