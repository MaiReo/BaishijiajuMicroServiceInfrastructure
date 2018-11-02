using Microsoft.AspNetCore.Hosting;
using System.Collections.Generic;
using System.Linq;

namespace Core.ServiceDiscovery
{
    /// <summary>
    /// 
    /// </summary>
    public class ServiceHelper : DefaultServiceHelper, IServiceHelper
    {
        private readonly IHostingEnvironment hostingEnvironment;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hostingEnvironment"></param>
        public ServiceHelper(IHostingEnvironment hostingEnvironment, ServiceDiscoveryConfiguration configuration) : base(configuration)
        {
            this.hostingEnvironment = hostingEnvironment;
        }

        protected override string ServiceName => hostingEnvironment.ApplicationName;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override IEnumerable<string> ServiceTags => Enumerable.Empty<string>();
    }
}
