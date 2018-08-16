using Autofac;
using Autofac.Extensions.DependencyInjection;
using Core.Abstractions;
using Core.Messages;
using Core.PersistentStore;
using Core.ServiceDiscovery;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class IntegrationServiceCollectionExtensions
    {
        public static void RegisterRequiredServices<TStartup>(this IServiceCollection services) where TStartup : class
        {
            services.AddOptions();
            services.AddRouting(options => options.LowercaseUrls = true);
            services.AddHttpContextAccessor();
            var mvcCoreBuilder = services.AddMvcCore();
            mvcCoreBuilder.AddApiExplorer();
            mvcCoreBuilder.AddApplicationPart(typeof(TStartup).Assembly);
            mvcCoreBuilder.AddFormatterMappings();
            mvcCoreBuilder.AddDataAnnotations();
            mvcCoreBuilder.SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            /*
             *  Bug found:
             *  Caused by: Swashbuckle.AspNetCore.Swagger
             *  Exception Type: System.MissingMethodException:
             *  Required: Microsoft.Net.Http.Headers.MediaTypeHeaderValue.Parse(System.String)
             *  Found: Microsoft.Net.Http.Headers.MediaTypeHeaderValue.Parse(Microsoft.Extensions.Primitives.StringSegment)
             *  To fix: Adds package Microsoft.AspNetCore.Mvc.Formatters.Json >= 2.0.0
             *  More detail: See https://team.baishijiaju.com:8443/tfs/HouseCoolCollection/Baishijiaju.Housecool%20v2.0/_wiki?pagePath=%2FFAQ%2F%E4%BD%BF%E7%94%A8AddMvcCore%E4%BB%A3%E6%9B%BFAddMvc
             */
            //TODO: 需要统一格式化时间字符串看情况再作处理
            mvcCoreBuilder.AddJsonFormatters(/*o => o.DateFormatString = "o"*/);
            mvcCoreBuilder.AddControllersAsServices();
        }
        /// <summary>
        /// Register dependencies to container.
        /// </summary>
        public static ContainerBuilder AddAutoFacWithConvention<TStartup>(this IServiceCollection services) where TStartup : class
        {
            var builder = new ContainerBuilder();
            if (services != null)
            {
                builder.Populate(services);
            }
            builder.RegisterModule<AbstractionModule>();
            builder.RegisterModule<RabbitMQModule>();
            builder.RegisterModule<ConsulModule>();
            builder.RegisterModule<EntityFrameworkCoreModule>();
            builder.RegisterAssemblyByConvention(typeof(IntegrationServiceCollectionExtensions).Assembly);
            builder.RegisterAssemblyByConvention(typeof(TStartup).Assembly);
            return builder;
        }
    }
}
