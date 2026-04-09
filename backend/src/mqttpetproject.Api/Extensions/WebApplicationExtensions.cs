using mqttpetproject.Api.Middlewares;

namespace mqttpetproject.Api.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication UseApiPipeline(this WebApplication app)
    {
        app.UseMiddleware<ExceptionHandlingMiddleware>();

        app.UseSwagger();
        app.UseSwaggerUI();

        app.MapControllers();
        app.MapHealthChecks("/health");

        return app;
    }
}
