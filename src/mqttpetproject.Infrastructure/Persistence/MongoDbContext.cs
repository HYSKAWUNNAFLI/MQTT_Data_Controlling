using Microsoft.Extensions.Options;
using MongoDB.Driver;
using mqttpetproject.Domain.Entities;
using mqttpetproject.Infrastructure.Configuration;

namespace mqttpetproject.Infrastructure.Persistence;

public sealed class MongoDbContext
{
    public MongoDbContext(IOptions<MongoDbOptions> options)
    {
        var mongoOptions = options.Value;
        var client = new MongoClient(mongoOptions.ConnectionString);

        Database = client.GetDatabase(mongoOptions.DatabaseName);
        TelemetryCollection = Database.GetCollection<TelemetryMessage>(mongoOptions.TelemetryCollection);
        DlqAuditCollection = Database.GetCollection<DlqAuditMessage>(mongoOptions.DlqCollection);
    }

    public IMongoDatabase Database { get; }

    public IMongoCollection<TelemetryMessage> TelemetryCollection { get; }

    public IMongoCollection<DlqAuditMessage> DlqAuditCollection { get; }
}
