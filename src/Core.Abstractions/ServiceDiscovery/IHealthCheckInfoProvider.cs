namespace Core.ServiceDiscovery
{
    public interface IHealthCheckInfoProvider
    {
        int Interval { get; }
    }
}
