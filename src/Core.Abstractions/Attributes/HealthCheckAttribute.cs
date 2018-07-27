using System;

namespace Core.ServiceDiscovery
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public class HealthCheckAttribute : Attribute, IHealthCheckInfoProvider
    {
        public HealthCheckAttribute():this(5)
        {
        }

        public HealthCheckAttribute(int interval)
        {
            this.Interval = interval;
        }

        public int Interval { get; }
    }
}
