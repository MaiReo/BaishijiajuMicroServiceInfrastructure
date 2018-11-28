using Abp;
using Abp.AspNetCore;
using Abp.Modules;
using Core.Dependency;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.AspNetCore
{
    public static class AbpAspNetCoreExtensions
    {

        public static IServiceProvider AddAbpWithCoreSessionCompatible<TStartupModule>(this IServiceCollection services, Action<AbpBootstrapperOptions> optionsAction = null) where TStartupModule : AbpModule
        {
            void config(AbpBootstrapperOptions options)
            {

                options.IocManager = new InlineDependenciesPropagatingIocManager();
                optionsAction?.Invoke(options);
            }
            return services.AddAbp<TStartupModule>(config);
        }
    }
}
