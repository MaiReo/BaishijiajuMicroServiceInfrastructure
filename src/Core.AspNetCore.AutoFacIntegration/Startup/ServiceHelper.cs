using Core.Abstractions.Dependency;
using Core.ServiceDiscovery;
using Microsoft.AspNetCore.Hosting;
using System.Collections.Generic;

namespace Core.Company.Account.Web.Startup
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
        public ServiceHelper(IHostingEnvironment hostingEnvironment)
        {
            this.hostingEnvironment = hostingEnvironment;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override string ServiceId => hostingEnvironment.ApplicationName;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override IEnumerable<string> ServiceTags => new[] { "core", "notification", "notification-apppush" };
    }
}
