namespace mqttpetproject.Infrastructure.Configuration;

public sealed class AppOptions
{
    public string Name { get; set; } = "mqttpetproject";
    public bool StoreDlqAudit { get; set; } = true;
}
