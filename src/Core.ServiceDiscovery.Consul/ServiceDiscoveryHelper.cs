using Consul;
using Core.Exceptions;
using Core.Utilities;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Core.ServiceDiscovery
{

    public class ServiceDiscoveryHelper : IServiceDiscoveryHelper
    {
        private readonly IServerAddressesFeature _serverAddressesFeature;
        private readonly IConsulClient _consulClient;
        private readonly IServiceHelper _serviceHelper;
        private readonly IHealthCheckHelper _healthCheckHelper;
        private readonly ServiceDiscoveryConfiguration _configuration;
        private readonly IServiceEndpointSelector _serviceEndpointSelector;
        private readonly RandomServiceEndpointSelector _randomServiceEndpointSelector;
        private readonly Random _random;

        public virtual bool IsRegistered { get; protected set; }

        public virtual ILogger Logger { get; protected set; }

        public ServiceDiscoveryHelper(
            IConsulClient consulClient,
            IServiceHelper serviceHelper,
            IHealthCheckHelper healthCheckHelper,
            ILogger<ServiceDiscoveryHelper> logger,
            ServiceDiscoveryConfiguration configuration,
            IServiceEndpointSelector serviceEndpointSelector,
            RandomServiceEndpointSelector randomServiceEndpointSelector,
            IServer server)
        {
            _random = new Random();
            _consulClient = consulClient;
            _serviceHelper = serviceHelper;
            _healthCheckHelper = healthCheckHelper;
            _configuration = configuration;
            _serviceEndpointSelector = serviceEndpointSelector;
            _randomServiceEndpointSelector = randomServiceEndpointSelector;
            _serverAddressesFeature = server.Features.Get<IServerAddressesFeature>();
            Logger = (ILogger)logger ?? NullLogger.Instance;
        }
        public virtual async ValueTask RegisterAsync()
        {
            if (!_configuration.AutoRegister)
            {
                return;
            }

            if (IsRegistered)
            {
                throw new InvalidOperationException("Service has been already registered to consul.");
            }
            var request = await BuildRegisterationRequestAsync();
            if (request == null)
            {
                throw new InvalidOperationException("Cannot register service to consul during buiding request.");
            }
            if (string.IsNullOrWhiteSpace(request.ID))
            {
                throw new InvalidOperationException("Cannot register service to consul. Unexpected service id.");
            }
            await _consulClient.Agent.ServiceDeregister(request.ID);
            await _consulClient.Agent.ServiceRegister(request);
            IsRegistered = true;
        }

        public virtual async ValueTask<bool> DeregisterAsync()
        {
            if (!IsRegistered)
            {
                return false;
            }
            var request = await BuildRegisterationRequestAsync();
            if (string.IsNullOrWhiteSpace(request?.ID))
            {
                return false;
            }

            await _consulClient.Agent.ServiceDeregister(request.ID);
            return true;
        }

        private AgentServiceRegistration registration;

#pragma warning disable CS1998
        private async ValueTask<AgentServiceRegistration> BuildRegisterationRequestAsync()
#pragma warning restore CS1998
        {
            if (registration != null)
            {
                return registration;
            }
            var serviceId = _serviceHelper.GetRunningServiceId();
            var serviceName = _serviceHelper.GetRunningServiceName();
            var serviceTags = _serviceHelper.GetRunningServiceTags().ToArray();

            var address = "http://localhost:5000";

            IPAddress addr = null;

            foreach (var addressString in _serverAddressesFeature.Addresses)
            {
                Logger.LogTrace($"Service discovery:Found address:{addressString}");
                if (addr != null)
                {
                    break;
                }

                address = addressString;

                if (address == null)
                {
                    continue;
                }

                if (address.StartsWith("http://+:") || address.StartsWith("https://+:"))
                {
                    address = address.Replace("+", "[::]");
                }

                if (!Uri.TryCreate(address, UriKind.Absolute, out var uri))
                {
                    continue;
                }

                if (IPAddress.TryParse(uri.DnsSafeHost, out addr))
                {
                    if (IPAddress.Loopback.Equals(addr) || IPAddress.IPv6Loopback.Equals(addr))
                    {
                        addr = null;
                        continue;
                    }
                    if (IPAddress.Any.Equals(addr) || IPAddress.IPv6Any.Equals(addr))
                    {
                        addr = IPAddress.Loopback;
                        break;
                    }
                }
                else
                {
                    addr = null;
                }
            }

            Logger.LogTrace($"Service discovery:Current IP address is : {addr}");
            Logger.LogTrace($"Service discovery:Nearest address:{address}");
            if (addr == null)
            {
                Logger.LogCritical($"Service discovery:No usable IP address.(0)");
                return null;
            }

            if (!Uri.TryCreate(address, UriKind.Absolute, out var addressUri))
            {
                Logger.LogCritical($"Service discovery:Cannot build a usable URI.");
                return null;
            }
            var hostNameOrAddress = string.Empty;

            if (IPAddress.Loopback.Equals(addr) || IPAddress.IPv6Loopback.Equals(addr))
            {
                //// lo(v6)
                hostNameOrAddress = Dns.GetHostName();
            }
            else
            {
                hostNameOrAddress = addr.ToString();
            }
            Logger.LogTrace($"Service discovery:Current host name is : {hostNameOrAddress}");
            Logger.LogTrace($"Service discovery:Current address uri is : {addressUri}");
            var ipEntry = await Dns.GetHostEntryAsync(hostNameOrAddress);
            Logger.LogTrace($"Service discovery:Found host name is : {ipEntry.HostName}");
            var addressListString = ipEntry.AddressList.Aggregate(new StringBuilder(), (sb, s) => sb.Append(s).Append(","));
            Logger.LogTrace($"Service discovery:Found all addresses are : {addressListString}");
            bool isIpv6 = false;

            var ipAddress = ipEntry.AddressList
                .Where(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                .Where(ip => !IPAddress.Any.Equals(ip))
                .Where(ip => !IPAddress.Loopback.Equals(ip))
                .Where(ip => !IPAddress.None.Equals(ip))
                .FirstOrDefault();

            if (ipAddress == null)
            {
                isIpv6 = true;
                ipAddress = ipEntry.AddressList
                .Where(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
                .Where(ip => !IPAddress.IPv6Any.Equals(ip))
                .Where(ip => !IPAddress.IPv6Loopback.Equals(ip))
                .Where(ip => !IPAddress.IPv6None.Equals(ip))
                .FirstOrDefault();
            }

            if (ipAddress == null)
            {
                Logger.LogCritical($"Service discovery:No usable IP address.(1)");
                return null;
            }
            var host = ipAddress.ToString();

            if (isIpv6)
            {
                host = $"[{host}]";
            }

            address = $"{addressUri.Scheme}://{host}:{addressUri.Port}";

            Logger.LogTrace($"Service discovery:Try resolving address:{address}");

            if (!Uri.TryCreate(address, UriKind.Absolute, out addressUri))
            {
                Logger.LogCritical($"Service discovery:No usable IP address.(2)");
                return null;
            }
            Logger.LogTrace($"Service discovery:The new address uri is :{addressUri}");
            var checks = _healthCheckHelper.GetHeathCheckInfo()
                .Select(info => new AgentServiceCheck
                {
                    HTTP = address + info.Path,
                    Interval = TimeSpan.FromSeconds(info.Interval),
                    Status = HealthStatus.Passing
                })
                .ToArray();

            var request = registration = new AgentServiceRegistration
            {
                ID = serviceId,
                Name = serviceName,
                Address = addressUri.DnsSafeHost,
                Port = addressUri.Port,
                Tags = serviceTags,
                Checks = checks
            };
            return request;
        }
        public string GetServiceBasePath(string serviceName, string scheme = "http://") => GetServiceBasePathAsync(serviceName, scheme).GetAwaiter().GetResult();

        public virtual async ValueTask<string> GetServiceBasePathAsync(string serviceName, string scheme = "http://", CancellationToken cancellationToken = default)
        {
            var (address, port) = await GetServiceAddressAsync(serviceName, cancellationToken);
            var serviceEndPoint = $"{scheme}{address}:{port}";
            return serviceEndPoint;
        }

        public (string Address, int Port) GetServiceAddress(string serviceName) => GetServiceAddressAsync(serviceName).GetAwaiter().GetResult();

        public async ValueTask<(string Address, int Port)> GetServiceAddressAsync(string serviceName, CancellationToken cancellationToken = default)
        {
            var services = await _consulClient.Catalog.Service(serviceName);
            if (services.Response == null | services.Response.Length == 0)
            {
                throw new BadGatewayException($"Cannot found service endpoint", serviceName);
            }
            //负载均衡随机算法
            var allServices = services.Response.Select(x => (x.ServiceAddress, x.ServicePort)).ToList();
            return _randomServiceEndpointSelector.SelectService(allServices).Model;
        }

        public async ValueTask<IDisposableModel<(string Address, int Port)>> GetServerAddressAndAddRefAsync(string serviceName, CancellationToken cancellationToken = default)
        {
            var services = await _consulClient.Catalog.Service(serviceName);
            if (services.Response == null | services.Response.Length == 0)
            {
                throw new BadGatewayException($"Cannot found service endpoint", serviceName);
            }
            var allServices = services.Response.Select(x => (x.ServiceAddress, x.ServicePort)).ToList();
            var service = _serviceEndpointSelector.SelectService(allServices);
            return service;
        }
    }
}
