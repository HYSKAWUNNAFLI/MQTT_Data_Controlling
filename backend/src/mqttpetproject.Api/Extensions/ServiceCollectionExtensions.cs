using mqttpetproject.Api.BackgroundServices;
using mqttpetproject.Application;
using mqttpetproject.Infrastructure;

namespace mqttpetproject.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        services.AddHealthChecks();

        services.AddApplication();
        services.AddInfrastructure(configuration);
        services.AddHostedService<RabbitMqConsumerBackgroundService>();

        return services;
    }
}
