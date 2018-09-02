﻿using System;
using System.Collections.Generic;
using System.Reflection;

namespace Core.ServiceDiscovery
{
    public class DefaultServiceHelper : IServiceHelper
    {
        public DefaultServiceHelper(ServiceDiscoveryConfiguration configuration)
        {
            Configuration = configuration;
        }

        public ServiceDiscoveryConfiguration Configuration { get; }

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

        string IServiceHelper.GetRunningServiceId() => Configuration.ServiceId ?? Normalize(ServiceId);

        string IServiceHelper.GetRunningServiceName() => Configuration.ServiceName ?? Normalize(ServiceName);

        IEnumerable<string> IServiceHelper.GetRunningServiceTags() => Configuration.ServiceTags ?? ServiceTags;
    }
}
