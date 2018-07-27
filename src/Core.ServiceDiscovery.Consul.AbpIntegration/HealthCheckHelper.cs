using Abp.Dependency;
using Core.ServiceDiscovery;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using System.Collections.Generic;
using System.Linq;

namespace Core.ServiceDiscovery
{
    public class HealthCheckHelper : AspNetHealthCheckHelper, IHealthCheckHelper, ISingletonDependency
    {
        private readonly IApiDescriptionGroupCollectionProvider apiExplorer;

        public HealthCheckHelper(IApiDescriptionGroupCollectionProvider apiExplorer)
        {
            this.apiExplorer = apiExplorer;
        }
        public override IEnumerable<ApiDescription> GetApiDescriptions()
        {
            return apiExplorer.ApiDescriptionGroups.Items.SelectMany(x => x.Items);
        }
    }
}
