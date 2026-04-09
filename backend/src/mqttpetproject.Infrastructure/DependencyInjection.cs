using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using mqttpetproject.Application.Abstractions.Messaging;
using mqttpetproject.Application.Abstractions.Persistence;
using mqttpetproject.Domain.Constants;
using mqttpetproject.Infrastructure.Configuration;
using mqttpetproject.Infrastructure.Messaging;
using mqttpetproject.Infrastructure.Persistence;
using mqttpetproject.Infrastructure.Persistence.Repositories;

namespace mqttpetproject.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AppOptions>(configuration.GetSection(RabbitMqConstants.AppConfigurationSection));
        services.Configure<RabbitMqOptions>(configuration.GetSection(RabbitMqConstants.RabbitMqConfigurationSection));
        services.Configure<MongoDbOptions>(configuration.GetSection(RabbitMqConstants.MongoDbConfigurationSection));

        services.PostConfigure<RabbitMqOptions>(options =>
        {
            options.Host = GetString(configuration, "RABBITMQ_HOST", options.Host);
            options.Port = GetInt32(configuration, "RABBITMQ_PORT", options.Port);
            options.Username = GetString(configuration, "RABBITMQ_USERNAME", options.Username);
            options.Password = GetString(configuration, "RABBITMQ_PASSWORD", options.Password);
            options.MainExchange = GetString(configuration, "RABBITMQ_MAIN_EXCHANGE", options.MainExchange);
            options.MainQueue = GetString(configuration, "RABBITMQ_MAIN_QUEUE", options.MainQueue);
            options.MainRoutingKey = GetString(configuration, "RABBITMQ_MAIN_ROUTING_KEY", options.MainRoutingKey);
            options.Dlx = GetString(configuration, "RABBITMQ_DLX", options.Dlx);
            options.Dlq = GetString(configuration, "RABBITMQ_DLQ", options.Dlq);
            options.DlqRoutingKey = GetString(configuration, "RABBITMQ_DLQ_ROUTING_KEY", options.DlqRoutingKey);
        });

        services.PostConfigure<MongoDbOptions>(options =>
        {
            options.ConnectionString = GetString(configuration, "MONGODB_CONNECTION_STRING", options.ConnectionString);
            options.DatabaseName = GetString(configuration, "MONGODB_DATABASE_NAME", options.DatabaseName);
            options.TelemetryCollection = GetString(configuration, "MONGODB_TELEMETRY_COLLECTION", options.TelemetryCollection);
            options.DlqCollection = GetString(configuration, "MONGODB_DLQ_COLLECTION", options.DlqCollection);
        });

        services.AddSingleton<MongoDbContext>();
        services.AddSingleton<RabbitMqConnectionFactory>();
        services.AddScoped<ITelemetryRepository, TelemetryRepository>();

        services.AddSingleton<IRabbitMqTopologyService, RabbitMqTopologyService>();
        services.AddSingleton<IMessagePublisher, RabbitMqPublisher>();
        services.AddSingleton<IDlqPublisher, RabbitMqDlqPublisher>();
        services.AddSingleton<IMessageConsumer, RabbitMqConsumer>();

        return services;
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
}
