using Abp.Dependency;
using Castle.MicroKernel.Registration;
using Core.Messages;

namespace Core.DependencyRegistrars
{
    public class MessageHandlerConventionalRegistrar : IConventionalDependencyRegistrar
    {
        public void RegisterAssembly(IConventionalRegistrationContext context)
        {
            context.IocManager.IocContainer.Register(
                 Types
                 .FromAssembly(context.Assembly)
                 .IncludeNonPublicTypes()
                 .BasedOn(typeof(IMessageHandler<>))
                 .OrBasedOn(typeof(IAsyncMessageHandler<>))
                 .WithServiceBase()
                 .WithServiceSelf()
                 .LifestyleTransient()
                );
        }
    }
}