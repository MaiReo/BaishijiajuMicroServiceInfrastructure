using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Core.ServiceDiscovery
{
    public class DefaultServiceHelper : IServiceHelper
    {
        public static DefaultServiceHelper Instance => new DefaultServiceHelper();

        protected virtual string ServiceId => Assembly.GetEntryAssembly().GetName().Name;

        protected virtual string ServiceName => ServiceId;

        protected virtual IEnumerable<string> ServiceTags => new[] { "core" };

        protected virtual string Normalize(in string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return Guid.NewGuid().ToString("N");
            }
            var newName = name.ToLowerInvariant().Replace('.', '-'); 
            return newName;
        }

        string IServiceHelper.GetRunningServiceId() => Normalize(ServiceId);

        string IServiceHelper.GetRunningServiceName() => Normalize(ServiceName);

        IEnumerable<string> IServiceHelper.GetRunningServiceTags()
        {
            foreach (var tag in ServiceTags)
            {
                yield return Normalize(tag);
            }
        }
    }
}
