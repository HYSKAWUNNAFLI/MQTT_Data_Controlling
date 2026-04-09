using mqttpetproject.Api.BackgroundServices;
using mqttpetproject.Api.Configuration;
using mqttpetproject.Infrastructure.Configuration;
using mqttpetproject.Infrastructure.Persistence;
using mqttpetproject.Infrastructure.Persistence.Repositories;
using mqttpetproject.Application.Abstractions.Persistence;
using mqttpetproject.Application.Abstractions.Services;
using mqttpetproject.Application.Abstractions.Validation;
using mqttpetproject.Application.Services;
using mqttpetproject.Application.Validation;
using mqttpetproject.Application.Validation.Rules;
using mqttpetproject.RabbitMqAdapter;

namespace mqttpetproject.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        services.AddHealthChecks();

        services.Configure<AppOptions>(configuration.GetSection("App"));
        services.Configure<MongoDbOptions>(configuration.GetSection("MongoDb"));
        services.PostConfigure<MongoDbOptions>(options =>
        {
            options.ConnectionString = GetString(configuration, "MONGODB_CONNECTION_STRING", options.ConnectionString);
            options.DatabaseName = GetString(configuration, "MONGODB_DATABASE_NAME", options.DatabaseName);
            options.TelemetryCollection = GetString(configuration, "MONGODB_TELEMETRY_COLLECTION", options.TelemetryCollection);
            options.DlqCollection = GetString(configuration, "MONGODB_DLQ_COLLECTION", options.DlqCollection);
        });

        services.AddSingleton<MongoDbContext>();
        services.AddScoped<ITelemetryRepository, TelemetryRepository>();
        services.AddScoped<ITelemetryProcessingService, TelemetryProcessingService>();
        services.AddScoped<ITelemetryValidator, TelemetryValidator>();
        services.AddScoped<ITopicRuleValidator, ElectricityTopicRuleValidator>();
        services.AddScoped<ITopicRuleValidator, SteamTopicRuleValidator>();
        services.AddScoped<ITopicRuleValidator, GasTopicRuleValidator>();
        services.AddSingleton<IIdGenerator, IdGenerator>();

        services.AddRabbitMqAdapter(configuration);
        services.AddHostedService<InboundMessageRuntimeBackgroundService>();

        return services;
    }

    private static string GetString(IConfiguration configuration, string key, string fallback)
    {
        var value = configuration[key];
        return string.IsNullOrWhiteSpace(value) ? fallback : value;
    }
}
