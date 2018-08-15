using Autofac;
using Core.Messages.Bus;
using Core.Messages.Bus.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Builder
{
    public static class IntegrationApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseMessageBus(this IApplicationBuilder app)
        {
            var messageBus = app.ApplicationServices.GetRequiredService<IMessageBus>();
            var componentContext = app.ApplicationServices.GetRequiredService<IComponentContext>();
            messageBus.RegisterMessageHandlers(componentContext);
            return app;
        }
    }
}
