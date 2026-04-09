# RabbitMQ Adapter

Thu muc nay chua RabbitMQ adapter bang C#/.NET, duoc tach khoi `backend/src`.

## Muc Dich

- Giu cho backend core khong phu thuoc truc tiep vao RabbitMQ.
- Dong goi topology, consumer va message runtime vao mot adapter rieng.
- Tao boundary ro rang de sau nay co the thay RabbitMQ bang Kafka adapter.

## Cau Truc

- `src/mqttpetproject.RabbitMqAdapter`: RabbitMQ connection factory, topology va consumer runtime.

## Topology Hien Tai

- Nhan message vao tu `amq.topic`.
- Dung exchange-to-exchange binding tu `amq.topic` sang cac exchange domain.
- Ho tro nhieu exchange va nhieu queue.
- Moi queue co `ConsumerCount` va `PrefetchCount` rieng.
- Moi consumer su dung `channel` rieng de co the chay song song an toan.
- Tat ca main queue dung chung mot DLQ: `factory.data.dlq`.
- Topology duoc cau hinh trong `backend/src/mqttpetproject.Api/appsettings*.json`.

## Ghi Chu

Thu muc nay khong con chua Docker hoac broker runtime asset.
Broker RabbitMQ se duoc dong goi sau, tach rieng voi adapter code.
