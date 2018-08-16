using Core.Session;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.TestBase
{

    public class UnitTestCoreSessionProvider : ICoreSessionProvider
    {
        public ICoreSession Session { get; private set; }

        public IDisposable Use(string cityId, Guid? brokerCompanyId)
        {
            lock (this)
            {
                var currentSession = Session;
                var newSession = new UnitTestCoreSession(cityId, brokerCompanyId);
                var disposable = new SessionRestore(() => Session = currentSession);
                this.Session = newSession;
                return disposable;
            }
        }

        internal void Restore(ICoreSession session)
        {
            lock (this)
            {
                this.Session = session;
            }
        }

        private class SessionRestore : IDisposable
        {

            #region IDisposable Support
            private bool disposedValue = false; // 要检测冗余调用
            private readonly Action _onDisposing;

            public SessionRestore(Action onDisposing)
            {
                this._onDisposing = onDisposing;
            }

            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        // TODO: 释放托管状态(托管对象)。
                        _onDisposing?.Invoke();
                    }

                    // TODO: 释放未托管的资源(未托管的对象)并在以下内容中替代终结器。
                    // TODO: 将大型字段设置为 null。

                    disposedValue = true;
                }
            }

            // TODO: 仅当以上 Dispose(bool disposing) 拥有用于释放未托管资源的代码时才替代终结器。
            // ~SessionRestore() {
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
}
