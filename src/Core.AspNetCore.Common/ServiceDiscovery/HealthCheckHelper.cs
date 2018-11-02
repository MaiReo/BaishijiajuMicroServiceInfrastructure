using Microsoft.AspNetCore.Mvc.ApiExplorer;
using System.Collections.Generic;
using System.Linq;

namespace Core.ServiceDiscovery
{
    /// <summary>
    /// 
    /// </summary>
    public class HealthCheckHelper : AspNetHealthCheckHelper, IHealthCheckHelper
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
