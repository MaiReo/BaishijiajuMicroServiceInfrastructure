namespace Core.ServiceDiscovery
{
    public class ServiceDiscoveryConfiguration
    {
        public const string DEFAULT_ADDRESS = "http://localhost:8500";
        public ServiceDiscoveryConfiguration()
        {
            Address = DEFAULT_ADDRESS;
        }
        public string Address { get; set; }
       
    }
}
