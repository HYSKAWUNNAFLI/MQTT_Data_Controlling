using mqttpetproject.Application.Abstractions.Services;

namespace mqttpetproject.Application.Services;

public sealed class IdGenerator : IIdGenerator
{
    public string NewId()
    {
        return Guid.NewGuid().ToString("N");
    }
}
