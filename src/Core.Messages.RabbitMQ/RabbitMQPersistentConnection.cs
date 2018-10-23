using Core.ServiceDiscovery;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Core.Messages
{
    public class RabbitMQPersistentConnection : IRabbitMQPersistentConnection
    {
        private readonly IConnectionFactoryResolver _connectionFactoryResolver;
        private readonly ServiceDiscoveryConfiguration _serviceDiscoveryConfiguration;
        private readonly ILogger _logger;
        private IConnection _connection;
        private bool _disposed;

        private readonly object sync_root = new object();

        public RabbitMQPersistentConnection(
            IConnectionFactoryResolver connectionFactoryResolver,
            ServiceDiscoveryConfiguration serviceDiscoveryConfiguration,
            ILogger<RabbitMQPersistentConnection> logger)
        {
            _connectionFactoryResolver = connectionFactoryResolver ?? throw new ArgumentNullException(nameof(connectionFactoryResolver));
            _serviceDiscoveryConfiguration = serviceDiscoveryConfiguration;
            _logger = (ILogger)logger ?? NullLogger.Instance;
        }

        public bool IsConnected
        {
            get
            {
                return _connection != null && _connection.IsOpen && !_disposed;
            }
        }

        public IModel CreateModel()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(_connection));
            }
            if (!IsConnected)
            {
                throw new InvalidOperationException("No RabbitMQ connections are available to perform this action");
            }

            return _connection.CreateModel();
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            try
            {
                _connection.Close();
                _connection.Dispose();
            }
            catch (IOException ex)
            {
                _logger.LogCritical("无法关闭到RabbitMQ的连接", ex);
            }
        }

        public bool TryConnect()
        {
            //consumer and others also retry to connect
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(_connection));
            }
            if (IsConnected)
            {
                return true;
            }
            _logger.LogInformation("RabbitMQ Client is trying to connect");
            lock (sync_root)
            {
                if (_connection != null)
                {
                    if (_connection.IsOpen)
                    {
                        _connection.Close();
                    }
                    _connection.Dispose();
                    _connection = null;
                }
                var delay = 1;
                while (!IsConnected)
                {
                    try
                    {
                        _connection = _connectionFactoryResolver.Resolve().CreateConnection(_serviceDiscoveryConfiguration.ServiceName);
                    }
                    catch (Exception e)
                    {
                        delay = delay << 1;
                        _logger.LogError(e, $"RabbitMQ connections could not be created and opened, retry after {delay}s");
                        Task.Delay(TimeSpan.FromSeconds(delay))
                            .GetAwaiter()
                            .GetResult();
                    }
                }

                _connection.ConnectionShutdown += OnConnectionShutdown;
                _connection.ConnectionBlocked += OnConnectionBlocked;

                _logger.LogInformation($"RabbitMQ persistent connection acquired a connection {_connection.Endpoint.HostName} and is subscribed to failure events");

                return true;
            }
        }

        private void OnConnectionBlocked(object sender, ConnectionBlockedEventArgs e)
        {
            if (_disposed)
            {
                return;
            }

            _logger.LogWarning("A RabbitMQ connection is blocked. Trying to re-connect...");

            TryConnect();
        }


        private void OnConnectionShutdown(object sender, ShutdownEventArgs reason)
        {
            if (_disposed)
            {
                return;
            }
            TryConnect();
        }
    }
}