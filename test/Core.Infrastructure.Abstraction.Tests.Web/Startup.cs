using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Core.Infrastructure.Abstraction.Tests.Web.EFCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Infrastructure_Abstraction.Tests.Web
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.RegisterRequiredServices(typeof(Startup).Assembly);

            var databaseId = Guid.NewGuid().ToString("N");

            services.AddDbContext<WebDbContext>(option=>option.UseInMemoryDatabase(databaseId));

            services.AddMessageBus(o => 
            {
                o.HostName = "localhost";
                o.VirtualHost = "/";
                o.ExchangeName = "";
                o.QueueName = "";
                o.UserName = "guest";
                o.Password = "guest";
            });
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            builder.AddCoreModules();
            builder.RegisterAssemblyByConvention(typeof(Startup).Assembly);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IApplicationLifetime applicationLifetime)
        {
            app.UseDeveloperExceptionPage();
            app.UseMvcWithDefaultRoute();
            app.UseMessageBus();
        }
    }
}
