using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using mqttpetproject.Infrastructure.Configuration;

namespace mqttpetproject.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class HealthController : ControllerBase
{
    private readonly AppOptions _appOptions;
    private readonly IHostEnvironment _hostEnvironment;

    public HealthController(IOptions<AppOptions> appOptions, IHostEnvironment hostEnvironment)
    {
        _appOptions = appOptions.Value;
        _hostEnvironment = hostEnvironment;
    }

    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            status = "Healthy",
            service = _appOptions.Name,
            environment = _hostEnvironment.EnvironmentName,
            utcNow = DateTime.UtcNow
        });
    }
}
