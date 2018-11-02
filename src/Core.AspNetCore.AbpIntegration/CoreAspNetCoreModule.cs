using Abp.Dependency;
using Castle.MicroKernel.Registration;
using Core.ServiceDiscovery;
using Core.Session;

namespace Abp.Modules
{
    [DependsOn(
        typeof(AbstractionModule),
        typeof(RabbitMQModule),
        typeof(ConsulModule)
        )]
    public class CoreAspNetCoreModule : AbpModule
    {
        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(CoreAspNetCoreModule).Assembly);
            IocManager.RegisterIfNot<IHealthCheckHelper, HealthCheckHelper>();
            IocManager.RegisterIfNot<IServiceHelper, ServiceHelper>();
            IocManager.RegisterIfNot<ICoreSessionProvider, HttpContextCoreSessionProvider>();
        }

        public override void PostInitialize()
        {
            if (!IocManager.IsRegistered<ICoreSession>())
            {
                IocManager.IocContainer.Register(
                    Component.For<ICoreSession>()
                    .UsingFactoryMethod(krnl => krnl.Resolve<ICoreSessionProvider>().Session)
                    .LifestyleTransient()
                    );
            }

        }
    }
}
