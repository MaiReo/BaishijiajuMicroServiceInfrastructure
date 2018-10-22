using Castle.MicroKernel.Registration;
using Core.Messages;

namespace Abp.Modules
{
    [DependsOn(typeof(AbstractionModule))]
    public class RabbitMQModule : AbpModule
    {
        public override void PreInitialize()
        {
            IocManager.Register<IMessagePublisherWrapper, RabbitMQMessagePublisherWrapper>(Dependency.DependencyLifeStyle.Transient);

            IocManager.Register<IMessageSubscriber, MessageSubscriber>(Dependency.DependencyLifeStyle.Singleton);
            IocManager.Register<IRabbitMQPersistentConnection, RabbitMQPersistentConnection>(Dependency.DependencyLifeStyle.Singleton);
            IocManager.Register<IRabbitMQWrapper, RabbitMQWrapper>(Dependency.DependencyLifeStyle.Singleton);
        }

        private IRabbitMQPersistentConnection _connection;

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(RabbitMQModule).Assembly);

            IocManager.IocContainer.Register(
                Component
                .For<IConnectionFactoryResolver, ConnectionFactoryResolver>()
                .ImplementedBy<ConnectionFactoryResolver>()
                .LifestyleSingleton()
                );
        }

        public override void PostInitialize()
        {
            //确保在启动时创建连接而不是在初次使用时
            _connection = IocManager.Resolve<IRabbitMQPersistentConnection>();
            _connection.TryConnect();
            //自动订阅
            IocManager.Resolve<IMessageSubscriber>().AutoSubscribe();
        }

        public override void Shutdown()
        {
            _connection?.Dispose();
        }
    }
}