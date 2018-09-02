using Core.Abstractions.Dependency;
using Core.ServiceDiscovery;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Web.Startup
{
    /// <summary>
    /// 
    /// </summary>
    public class ServiceHelper : DefaultServiceHelper, IServiceHelper, ILifestyleSingleton
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
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override string ServiceId => Guid.NewGuid().ToString("N");


        protected override string ServiceName => hostingEnvironment.ApplicationName;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override IEnumerable<string> ServiceTags => Enumerable.Empty<string>();
    }
}
