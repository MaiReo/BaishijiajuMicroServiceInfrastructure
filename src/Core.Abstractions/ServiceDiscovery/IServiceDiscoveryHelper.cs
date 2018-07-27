using System.Threading.Tasks;

namespace Core.ServiceDiscovery
{
    public interface IServiceDiscoveryHelper
    {
        bool IsRegistered { get; }

        Task RegisterAsync();

        Task<bool> DeregisterAsync();
    }
}
