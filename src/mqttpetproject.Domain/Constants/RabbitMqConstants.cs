namespace mqttpetproject.Domain.Constants;

public static class RabbitMqConstants
{
    public const string RabbitMqConfigurationSection = "RabbitMq";
    public const string MongoDbConfigurationSection = "MongoDb";
    public const string AppConfigurationSection = "App";

    public const string MainExchange = "factory.data.exchange";
    public const string Dlx = "factory.data.dlx";
    public const string MainQueue = "factory.data.queue";
    public const string Dlq = "factory.data.dlq";
    public const string MainRoutingKey = "factory.telemetry";
    public const string DlqRoutingKey = "factory.telemetry.dlq";
}
