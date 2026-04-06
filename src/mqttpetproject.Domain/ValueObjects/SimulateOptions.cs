namespace mqttpetproject.Domain.ValueObjects;

public sealed class SimulateOptions
{
    public bool NoSave { get; set; }
    public bool ForceDbError { get; set; }
}
