# mqttpetproject

ASP.NET Core Web API backend for smart-factory telemetry processing with RabbitMQ, MongoDB, and topic-based validation.

## What It Does

- Declares RabbitMQ topology in code on startup.
- Consumes `factory.data.queue` via `BackgroundService`.
- Validates telemetry payloads in two layers:
  - schema validation
  - topic-specific domain rules through strategy validators
- Saves valid telemetry to MongoDB collection `telemetry_raw`.
- Saves DLQ audit records to MongoDB collection `telemetry_dlq_audit`.
- Returns `ACK`, `REJECT to DLQ`, or `NACK requeue` based on the processing result.

## RabbitMQ Topology

- Main exchange: `factory.data.exchange`
- Main queue: `factory.data.queue`
- Main routing key: `factory.telemetry`
- DLX: `factory.data.dlx`
- DLQ: `factory.data.dlq`
- DLQ routing key: `factory.telemetry.dlq`

The main queue is declared with:

- `x-dead-letter-exchange = factory.data.dlx`
- `x-dead-letter-routing-key = factory.telemetry.dlq`

## Run Locally

1. Copy `.env.example` to `.env` if you need a fresh local env file.
2. Start the stack:

```bash
docker compose -f docker-compose.yaml up --build
```

3. Open:
   - API: `http://localhost:8080`
   - Swagger: `http://localhost:8080/swagger`
   - RabbitMQ Management: `http://localhost:15672`

## MQTT / Node-RED Assumption

The compose setup enables the RabbitMQ MQTT plugin so Node-RED can connect to RabbitMQ on port `1883`. The backend itself still consumes from the AMQP queue `factory.data.queue`, so Node-RED or your broker-side routing must ensure the test payload ultimately reaches `factory.data.exchange` with routing key `factory.telemetry`.

## Tests

```bash
dotnet test
```
