using Autofac;
using Core.Messages.Bus;
using Core.Messages.Bus.Extensions;
using Core.PersistentStore;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Builder
{
    public static class IntegrationApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseMessageBus(this IApplicationBuilder app)
        {
            var lifetimeScope = app.ApplicationServices.GetRequiredService<ILifetimeScope>();
            var messageBus = lifetimeScope.Resolve<IMessageBus>();
            messageBus.RegisterMessageHandlers(lifetimeScope);
            return app;
        }
    }
}
