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

        public (string Address, int Port) GetServiceAddress(string serviceName)
        {
            return (default, default);
        }

        public ValueTask<(string Address, int Port)> GetServiceAddressAsync(string serviceName, CancellationToken cancellationToken = default)
        {
            return new ValueTask<(string Address, int Port)>((default, default));
        }

        public static NullServiceDiscoveryHelper Instance => new NullServiceDiscoveryHelper();

        public bool IsRegistered => false;
    }
}
