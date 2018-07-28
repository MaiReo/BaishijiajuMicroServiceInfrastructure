using Autofac;
using Core.Messages.Bus;
using Core.ServiceDiscovery;
using Core.Session;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Core.Abstractions.TestBase
{
    public abstract class AbstractionTestBase<TStartup> : IDisposable where TStartup : class
    {

        private readonly object _scopeObject;

        private IContainer _iocContainer;

        private ILifetimeScope _lifetimeScope;

        protected IComponentContext IocResolver => UseScopedResover ? _lifetimeScope : _iocContainer;

        public AbstractionTestBase()
        {
            _scopeObject = new object();
            this._iocContainer = RegisterRequiredServices(new ServiceCollection()).Build();
            this._lifetimeScope = _iocContainer.BeginLifetimeScope(_scopeObject);
            ConstructProperties();
        }

        public IMessageBus MessageBus { get; private set; }

        protected virtual bool UseScopedResover => true;

        protected internal virtual ContainerBuilder RegisterRequiredServices(IServiceCollection services)
        {
            services.AddDistributedMemoryCache();
            services.RegisterRequiredServices<TStartup>();
            services.AddMessageBus(o =>
            {
                o.ExchangeName = TestConsts.MESSAGE_BUS_EXCHANGE;
                o.HostName = TestConsts.MESSAGE_BUS_HOST;
                o.Password = TestConsts.MESSAGE_BUS_PWD;
                o.QueueName = TestConsts.MESSAGE_BUS_QUEUE;
                o.UserName = TestConsts.MESSAGE_BUS_USER;
                o.VirtualHost = TestConsts.MESSAGE_BUS_VHOST;
            });
            services.AddServiceDiscovery(o => o.Address = ServiceDiscoveryConfiguration.DEFAULT_ADDRESS);

            var containerBuilder = services.AddAutoFacWithConvention<TStartup>();
            var startupAssembly = typeof(TStartup).Assembly;
            var thisAssembly = typeof(AbstractionTestBase<>).Assembly;
            var runtimeThisAssembly = this.GetType().Assembly;
            containerBuilder.RegisterAssemblyByConvention(thisAssembly);
            if (runtimeThisAssembly != startupAssembly)
            {
                containerBuilder.RegisterAssemblyByConvention(runtimeThisAssembly);
            }
            containerBuilder
                .RegisterInstance(TestSession.Instance)
                .As<ICoreSession>();

            RegisterDependency(containerBuilder);
            return containerBuilder;
        }

        /// <summary>
        /// To register something needed for tests, override this method.
        /// </summary>
        /// <param name="builder"></param>
        protected virtual void RegisterDependency(ContainerBuilder builder)
        {
            // classes inhert from this my override this method to register something needed.
        }

        private void ConstructProperties()
        {
            MessageBus = Resolve<IMessageBus>();
        }

        /// <summary>
        /// Shortcut for resolving a object from IocContainer.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        protected T Resolve<T>() => IocResolver.Resolve<T>();


        #region IDisposable Support
        private bool disposedValue = false; // 要检测冗余调用

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)。
                    _lifetimeScope.Dispose();
                    _iocContainer.Dispose();
                }

                // TODO: 释放未托管的资源(未托管的对象)并在以下内容中替代终结器。
                // TODO: 将大型字段设置为 null。
                _lifetimeScope = null;
                _iocContainer = null;
                disposedValue = true;
            }
        }

        // TODO: 仅当以上 Dispose(bool disposing) 拥有用于释放未托管资源的代码时才替代终结器。
        // ~AbstractionTestBase() {
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
