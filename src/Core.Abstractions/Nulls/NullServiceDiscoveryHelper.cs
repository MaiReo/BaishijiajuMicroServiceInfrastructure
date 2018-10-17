using System.Threading;
using System.Threading.Tasks;

namespace Core.ServiceDiscovery
{
    public class NullServiceDiscoveryHelper : IServiceDiscoveryHelper
    {
        public void Register()
        {
            //No Actions.
        }

        public ValueTask RegisterAsync()
        {
            return new ValueTask();
        }

        public ValueTask<bool> DeregisterAsync()
        {
            return new ValueTask<bool>(true);
        }

        public ValueTask<string> GetServiceBasePathAsync(string serviceName, string scheme = "http://", CancellationToken cancellationToken = default)
        {
            return new ValueTask<string>(string.Concat(scheme, serviceName));
        }

        public string GetServiceBasePath(string serviceName, string scheme = "http://")
        {
            return string.Concat(scheme, serviceName);
        }

        public static NullServiceDiscoveryHelper Instance => new NullServiceDiscoveryHelper();

        public bool IsRegistered => false;
    }
}
