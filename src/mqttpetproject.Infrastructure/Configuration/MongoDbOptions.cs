using mqttpetproject.Infrastructure.Persistence.Collections;

namespace mqttpetproject.Infrastructure.Configuration;

public sealed class MongoDbOptions
{
    public string ConnectionString { get; set; } = "mongodb://mongodb:27017";
    public string DatabaseName { get; set; } = "smartfactory";
    public string TelemetryCollection { get; set; } = MongoCollectionNames.TelemetryRaw;
    public string DlqCollection { get; set; } = MongoCollectionNames.TelemetryDlqAudit;
}
