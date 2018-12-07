using Autofac;
using Core.Infrastructure.Abstraction.Tests.Web.EFCore;
using Core.Web.Startup;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Core.Infrastructure_Abstraction.Tests.Web
{
    public class Startup
    {
        private static string ProductVersion => typeof(Startup).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.RegisterRequiredServices(typeof(Startup).Assembly);

            var databaseId = Guid.NewGuid().ToString("N");

            services.AddDbContext<WebDbContext>(option => option.UseInMemoryDatabase(databaseId));

            services.AddMessageBus("192.168.0.252", 5672, "/", "debug.core.abstractions", "debug.core.abstractions", "guest", "guest");

            services.AddServiceDiscovery("http://192.168.0.252:8500", "debug.core.abstractions");

            // Swagger - Enable this line and the related lines in Configure method to enable swagger UI
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new Info
                {
                    Title = "Test API",
                    Version = ProductVersion
                }
                );
                options.DocInclusionPredicate((docName, description) => true);

                options.OperationFilter<RemovePreFixOperationFilter>();

                var xmlDocuments = new[] { "Core.Infrastructure.Abstraction.Tests.Web.xml" };
                var currentDirectory = Directory.GetCurrentDirectory();
                foreach (var xmlDocument in xmlDocuments)
                {
                    var path = Path.Combine(currentDirectory, xmlDocument);
                    if (File.Exists(path))
                    {
                        options.IncludeXmlComments(path);
                    }
                }
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
            app.UseRabbitMQWithAutoSubscribe(applicationLifetime.ApplicationStopping);

            // Enable middleware to serve generated Swagger as a JSON endpoint
            app.UseSwagger(options =>
            {
                options.PreSerializeFilters.Add((s, req) => s.Host = req.Host.Value);
                options.PreSerializeFilters.Add((s, req) => s.Schemes = new List<string> { req.Scheme });
            });
            // Enable middleware to serve swagger-ui assets (HTML, JS, CSS etc.)
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", $"Test API v{ProductVersion}");
            }); // URL: /swagger
        }
    }
}
