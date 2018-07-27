using Core.Abstractions.Dependency;
using Core.ServiceDiscovery;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using System.Collections.Generic;
using System.Linq;

namespace Core.Web.Startup
{
    /// <summary>
    /// 
    /// </summary>
    public class HealthCheckHelper : AspNetHealthCheckHelper, IHealthCheckHelper, ILifestyleSingleton
    {
        private readonly IApiDescriptionGroupCollectionProvider apiExplorer;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="apiExplorer"></param>
        public HealthCheckHelper(IApiDescriptionGroupCollectionProvider apiExplorer)
        {
            this.apiExplorer = apiExplorer;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<ApiDescription> GetApiDescriptions()
        {
            return apiExplorer.ApiDescriptionGroups.Items.SelectMany(x => x.Items);
        }
    }
}
