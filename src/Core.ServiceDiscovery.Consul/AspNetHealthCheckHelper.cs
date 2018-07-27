using Microsoft.AspNetCore.Mvc.ApiExplorer;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;

namespace Core.ServiceDiscovery
{
    public class AspNetHealthCheckHelper : IHealthCheckHelper
    {
        public virtual IEnumerable<ApiDescription> GetApiDescriptions()
        {
            return Enumerable.Empty<ApiDescription>();
        }

        public IEnumerable<IHealthCheckInfo> GetHeathCheckInfo()
        {
            foreach (var api in GetApiDescriptions())
            {
                if (api.ParameterDescriptions.Any())
                {
                    continue;
                }
                if (api.HttpMethod != null && api.HttpMethod != HttpMethod.Get.Method)
                {
                    continue;
                }
                var actionDescriptorType = api.ActionDescriptor.GetType();
                if (actionDescriptorType.Name != "ControllerActionDescriptor")
                {
                    continue;
                }
                var methodInfo = actionDescriptorType.GetProperty("MethodInfo")?.GetValue(api.ActionDescriptor) as MethodInfo;
                if (methodInfo == null)
                {
                    continue;
                }
                var healthDef = methodInfo.GetCustomAttributes(false).OfType<IHealthCheckInfoProvider>().FirstOrDefault();
                if (healthDef == null)
                {
                    continue;
                }
                var healthInfo = new HealthCheckInfo()
                {
                    Path = EnsureStartWithSlash(api.RelativePath),
                    Interval = healthDef.Interval
                };
                yield return healthInfo;
            }

        }

        static string EnsureStartWithSlash(string s)
        {
            if (s.StartsWith("/"))
            {
                return s;
            }
            return string.Concat("/", s);
        }
    }
}
