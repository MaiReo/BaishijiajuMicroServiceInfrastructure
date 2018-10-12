namespace Abp.Modules
{
    [DependsOn(
        typeof(AbstractionModule),
        typeof(RabbitMQModule),
        typeof(ConsulModule)
        )]
    public class CoreAspNetCoreModule : AbpModule
    {
    }
}
