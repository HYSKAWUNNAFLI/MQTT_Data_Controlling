using Microsoft.Extensions.DependencyInjection;
using mqttpetproject.Application.Abstractions.Services;
using mqttpetproject.Application.Abstractions.Validation;
using mqttpetproject.Application.Services;
using mqttpetproject.Application.Validation;
using mqttpetproject.Application.Validation.Rules;

namespace mqttpetproject.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ITelemetryProcessingService, TelemetryProcessingService>();
        services.AddScoped<ITelemetryValidator, TelemetryValidator>();

        services.AddScoped<ITopicRuleValidator, ElectricityTopicRuleValidator>();
        services.AddScoped<ITopicRuleValidator, SteamTopicRuleValidator>();
        services.AddScoped<ITopicRuleValidator, GasTopicRuleValidator>();

        services.AddSingleton<IIdGenerator, IdGenerator>();

        return services;
    }
}
