# mqttpetproject Backend

ASP.NET Core Web API backend cho smart-factory telemetry processing.

## Trach Nhiem

- Cung cap Web API host va health endpoint.
- Chua backend core de deserialize, validate va xu ly telemetry.
- Validate schema va domain rule theo topic.
- Luu telemetry hop le vao MongoDB.
- Quyết dinh `ACK`, `REJECT to DLQ`, hoac `NACK requeue`.

## Cau Truc

- `src/mqttpetproject.Api`: Web API va composition root.
- `src/mqttpetproject.Application`: contract, DTO, interface va business processing.
- `src/mqttpetproject.Domain`: domain model, enum va shared exception.
- `src/mqttpetproject.Infrastructure`: MongoDB persistence.
- `tests/mqttpetproject.Tests`: Unit tests.

## Chay Backend Rieng

Dung RabbitMQ broker tach rieng tren cong `5672` va MongoDB o `27017`.

1. Chuan bi bien moi truong theo `backend/.env.example`.
2. Chay API:

```bash
dotnet run --project src/mqttpetproject.Api
```

3. Chay test:

```bash
dotnet test mqttpetproject.sln
```

## Ghi Chu

RabbitMQ khong con nam trong `backend/src`.
Adapter RabbitMQ bang C#/.NET da duoc tach sang [rabbitmq/](/Users/ductranphamminh/Documents/VNTT/mqttpetproject/rabbitmq).
Topology multi-exchange va multi-queue duoc cau hinh trong `src/mqttpetproject.Api/appsettings*.json`.
