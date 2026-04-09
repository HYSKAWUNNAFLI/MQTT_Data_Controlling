using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using mqttpetproject.Application.Abstractions.Persistence;
using mqttpetproject.Domain.Entities;
using mqttpetproject.Domain.Exceptions;

namespace mqttpetproject.Infrastructure.Persistence.Repositories;

public sealed class TelemetryRepository : ITelemetryRepository
{
    private readonly MongoDbContext _mongoDbContext;
    private readonly ILogger<TelemetryRepository> _logger;

    public TelemetryRepository(MongoDbContext mongoDbContext, ILogger<TelemetryRepository> logger)
    {
        _mongoDbContext = mongoDbContext;
        _logger = logger;
    }

    public async Task SaveTelemetryAsync(TelemetryMessage message, CancellationToken cancellationToken = default)
    {
        try
        {
            await _mongoDbContext.TelemetryCollection.InsertOneAsync(message, cancellationToken: cancellationToken);
        }
        catch (MongoConnectionException exception)
        {
            _logger.LogWarning(exception, "Transient MongoDB connection issue while saving telemetry MessageId {MessageId}.", message.MessageId);
            throw new TransientPersistenceException("MongoDB connection failure.", exception);
        }
        catch (MongoExecutionTimeoutException exception)
        {
            _logger.LogWarning(exception, "MongoDB timeout while saving telemetry MessageId {MessageId}.", message.MessageId);
            throw new TransientPersistenceException("MongoDB execution timeout.", exception);
        }
    }

    public async Task SaveDlqAuditAsync(DlqAuditMessage message, CancellationToken cancellationToken = default)
    {
        try
        {
            await _mongoDbContext.DlqAuditCollection.InsertOneAsync(message, cancellationToken: cancellationToken);
        }
        catch (MongoConnectionException exception)
        {
            _logger.LogWarning(exception, "Transient MongoDB connection issue while saving DLQ audit MessageId {MessageId}.", message.MessageId);
            throw new TransientPersistenceException("MongoDB connection failure.", exception);
        }
        catch (MongoExecutionTimeoutException exception)
        {
            _logger.LogWarning(exception, "MongoDB timeout while saving DLQ audit MessageId {MessageId}.", message.MessageId);
            throw new TransientPersistenceException("MongoDB execution timeout.", exception);
        }
    }
}
