using System.Threading.Tasks;

namespace Core.ServiceDiscovery
{
    public class NullServiceDiscoveryHelper : IServiceDiscoveryHelper
    {
        public void Register()
        {
            //No Actions.
        }

        public Task RegisterAsync()
        {
            return Task.CompletedTask;
        }

        public Task<bool> DeregisterAsync()
        {
            return Task.FromResult(true);
        }

        public static NullServiceDiscoveryHelper Instance => new NullServiceDiscoveryHelper();

        public bool IsRegistered => false;
    }
}
