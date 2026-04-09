# mqttpetproject Monorepo

Repo nay da duoc tach thanh hai phan ro rang nhung van dung chung mot git repository:

- `backend/`: backend thuần, chua API host, contracts, domain, processing logic va persistence MongoDB.
- `rabbitmq/`: RabbitMQ adapter bang C#/.NET, tach khoi `backend/src` de co the thay bang Kafka adapter sau nay.

## Cau Truc Thu Muc

```text
.
|-- backend/
|   |-- mqttpetproject.sln
|   |-- src/
|   `-- tests/
`-- rabbitmq/
    `-- src/
```

## Dinh Huong Kien Truc

- `backend/src/mqttpetproject.Application`: chi chua contract va interface.
- `backend/src/mqttpetproject.Backend`: chua processing service, validator va topic rule.
- `backend/src/mqttpetproject.Infrastructure`: chi chua persistence MongoDB.
- `rabbitmq/src/mqttpetproject.RabbitMqAdapter`: chua topology, consumer va runtime RabbitMQ.

## Chay Local

1. Khoi dong RabbitMQ broker va MongoDB theo cach ban muon.
2. Cap nhat bien moi truong theo [backend/.env.example](/Users/ductranphamminh/Documents/VNTT/mqttpetproject/backend/.env.example).
3. Chay API:

```bash
cd backend
dotnet run --project src/mqttpetproject.Api
```
