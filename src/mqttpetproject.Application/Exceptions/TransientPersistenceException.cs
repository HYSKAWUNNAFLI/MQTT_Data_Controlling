namespace mqttpetproject.Application.Exceptions;

public sealed class TransientPersistenceException : Exception
{
    public TransientPersistenceException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}
