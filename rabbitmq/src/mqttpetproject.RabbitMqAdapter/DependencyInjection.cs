using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using mqttpetproject.Application.Abstractions.Messaging;
using mqttpetproject.RabbitMqAdapter.Configuration;
using mqttpetproject.RabbitMqAdapter.Hosting;
using mqttpetproject.RabbitMqAdapter.Messaging;

namespace mqttpetproject.RabbitMqAdapter;

public static class DependencyInjection
{
    public static IServiceCollection AddRabbitMqAdapter(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RabbitMqOptions>(configuration.GetSection(RabbitMqOptions.ConfigurationSectionName));
        services.PostConfigure<RabbitMqOptions>(options =>
        {
            options.Host = GetString(configuration, "RABBITMQ_HOST", options.Host);
            options.Port = GetInt32(configuration, "RABBITMQ_PORT", options.Port);
            options.Username = GetString(configuration, "RABBITMQ_USERNAME", options.Username);
            options.Password = GetString(configuration, "RABBITMQ_PASSWORD", options.Password);
            EnsureLegacyTopology(options, configuration);
        });

        services.AddSingleton<RabbitMqConnectionFactory>();
        services.AddSingleton<RabbitMqTopologyService>();
        services.AddSingleton<RabbitMqConsumer>();
        services.AddSingleton<IInboundMessageRuntime, RabbitMqMessageRuntime>();

        return services;
    }

    private static void EnsureLegacyTopology(RabbitMqOptions options, IConfiguration configuration)
    {
        if (options.Exchanges.Count > 0 || options.Queues.Count > 0 || options.ExchangeBindings.Count > 0)
        {
            return;
        }

        var deadLetterExchange = GetString(configuration, "RABBITMQ_DLX", "factory.data.dlx");
        var deadLetterRoutingKey = GetString(configuration, "RABBITMQ_DLQ_ROUTING_KEY", "factory.telemetry.dlq");
        var prefetchCount = GetUInt16(configuration, "RABBITMQ_PREFETCH_COUNT", 1);
        var maxRetryAttempts = GetInt32(configuration, "RABBITMQ_MAX_RETRY_ATTEMPTS", 3);

        options.Exchanges = new List<RabbitMqExchangeDefinition>
        {
            new RabbitMqExchangeDefinition
            {
                Name = "amq.topic",
                Type = "topic",
                Declare = false
            },
            new RabbitMqExchangeDefinition
            {
                Name = "factory.electricity.exchange",
                Type = "topic"
            },
            new RabbitMqExchangeDefinition
            {
                Name = "factory.steam.exchange",
                Type = "topic"
            },
            new RabbitMqExchangeDefinition
            {
                Name = "factory.gas.exchange",
                Type = "topic"
            },
            new RabbitMqExchangeDefinition
            {
                Name = deadLetterExchange,
                Type = "direct"
            }
        };

        options.ExchangeBindings = new List<RabbitMqExchangeBindingDefinition>
        {
            new()
            {
                SourceExchange = "amq.topic",
                DestinationExchange = "factory.electricity.exchange",
                RoutingKey = "factory.telemetry.electricity.#"
            },
            new()
            {
                SourceExchange = "amq.topic",
                DestinationExchange = "factory.steam.exchange",
                RoutingKey = "factory.telemetry.steam.#"
            },
            new()
            {
                SourceExchange = "amq.topic",
                DestinationExchange = "factory.gas.exchange",
                RoutingKey = "factory.telemetry.gas.#"
            }
        };

        options.Queues = new List<RabbitMqQueueDefinition>
        {
            new RabbitMqQueueDefinition
            {
                Name = "factory.telemetry.electricity.queue",
                PrefetchCount = prefetchCount,
                ConsumerCount = 2,
                MaxRetryAttempts = maxRetryAttempts,
                DeadLetterExchange = deadLetterExchange,
                DeadLetterRoutingKey = deadLetterRoutingKey,
                Bindings = new List<RabbitMqBindingDefinition>
                {
                    new RabbitMqBindingDefinition
                    {
                        Exchange = "factory.electricity.exchange",
                        RoutingKey = "factory.telemetry.electricity.#"
                    }
                }
            },
            new RabbitMqQueueDefinition
            {
                Name = "factory.telemetry.steam.queue",
                PrefetchCount = prefetchCount,
                ConsumerCount = 2,
                MaxRetryAttempts = maxRetryAttempts,
                DeadLetterExchange = deadLetterExchange,
                DeadLetterRoutingKey = deadLetterRoutingKey,
                Bindings = new List<RabbitMqBindingDefinition>
                {
                    new RabbitMqBindingDefinition
                    {
                        Exchange = "factory.steam.exchange",
                        RoutingKey = "factory.telemetry.steam.#"
                    }
                }
            },
            new RabbitMqQueueDefinition
            {
                Name = "factory.telemetry.gas.queue",
                PrefetchCount = prefetchCount,
                ConsumerCount = 2,
                MaxRetryAttempts = maxRetryAttempts,
                DeadLetterExchange = deadLetterExchange,
                DeadLetterRoutingKey = deadLetterRoutingKey,
                Bindings = new List<RabbitMqBindingDefinition>
                {
                    new RabbitMqBindingDefinition
                    {
                        Exchange = "factory.gas.exchange",
                        RoutingKey = "factory.telemetry.gas.#"
                    }
                }
            },
            new RabbitMqQueueDefinition
            {
                Name = GetString(configuration, "RABBITMQ_DLQ", "factory.data.dlq"),
                PrefetchCount = prefetchCount,
                ConsumerCount = 0,
                Bindings = new List<RabbitMqBindingDefinition>
                {
                    new RabbitMqBindingDefinition
                    {
                        Exchange = deadLetterExchange,
                        RoutingKey = deadLetterRoutingKey
                    }
                }
            }
        };
    }

    private static string GetString(IConfiguration configuration, string key, string fallback)
    {
        var value = configuration[key];
        return string.IsNullOrWhiteSpace(value) ? fallback : value;
    }

    private static int GetInt32(IConfiguration configuration, string key, int fallback)
    {
        return int.TryParse(configuration[key], out var value) ? value : fallback;
    }

    private static ushort GetUInt16(IConfiguration configuration, string key, ushort fallback)
    {
        return ushort.TryParse(configuration[key], out var value) ? value : fallback;
    }
}
