using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using mqttpetproject.RabbitMqAdapter.Configuration;

namespace mqttpetproject.RabbitMqAdapter.Messaging;

public sealed class RabbitMqConnectionFactory : IDisposable
{
    private readonly ConnectionFactory _connectionFactory;
    private readonly object _sync = new();
    private IConnection? _connection;

    public RabbitMqConnectionFactory(IOptions<RabbitMqOptions> options)
    {
        var rabbitMqOptions = options.Value;

        _connectionFactory = new ConnectionFactory
        {
            HostName = rabbitMqOptions.Host,
            Port = rabbitMqOptions.Port,
            UserName = rabbitMqOptions.Username,
            Password = rabbitMqOptions.Password,
            DispatchConsumersAsync = true,
            AutomaticRecoveryEnabled = true,
            TopologyRecoveryEnabled = true
        };
    }

    public IConnection CreateConnection()
    {
        if (_connection is { IsOpen: true })
        {
            return _connection;
        }

        lock (_sync)
        {
            if (_connection is { IsOpen: true })
            {
                return _connection;
            }

            _connection?.Dispose();
            _connection = _connectionFactory.CreateConnection();
            return _connection;
        }
    }

    public IModel CreateModel()
    {
        return CreateConnection().CreateModel();
    }

    public void Dispose()
    {
        lock (_sync)
        {
            _connection?.Dispose();
            _connection = null;
        }
    }
}
