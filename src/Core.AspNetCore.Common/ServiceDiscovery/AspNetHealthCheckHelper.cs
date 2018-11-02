using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace Core.ServiceDiscovery
{
    public abstract class AspNetHealthCheckHelper : IHealthCheckHelper
    {
        public abstract IEnumerable<ApiDescription> GetApiDescriptions();

        public virtual IEnumerable<IHealthCheckInfo> GetHeathCheckInfo()
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
                if (!(api.ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor))
                {
                    continue;
                }
                var methodInfo = controllerActionDescriptor.MethodInfo;
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

        private static string EnsureStartWithSlash(string s)
        {
            if (s.StartsWith("/"))
            {
                return s;
            }
            return string.Concat("/", s);
        }
    }
}
