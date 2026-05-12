# Visão Geral

## O que é este projeto?

O `transaction-api` é o **serviço de entrada** de um ecossistema de pagamentos instantâneos estilo PIX. Ele é responsável por:

- Receber e validar a intenção de pagamento
- Persistir a transação com status `PENDING`
- Publicar o evento `TransactionCreated` para que outros serviços continuem o fluxo

Ele **não** processa o débito/crédito — essa responsabilidade pertence ao `Ledger Service`.

---

## Contexto no Ecossistema

```
[Cliente] → [API Gateway] → [Auth Service]
                                │
                                ▼
                      [Transaction API]  ◀── este projeto
                                │
                         publica evento
                                │
              ┌─────────────────┼──────────────────┐
              ▼                 ▼                  ▼
       [Ledger Service]  [Fraud Detection]  [BACEN Connector]
              │
    ┌─────────┴─────────┐
    ▼                   ▼
[Notification]     [Key Manager]
```

---

## Objetivos de Design

| Objetivo | Como foi endereçado |
|----------|--------------------|
| Rastreabilidade completa | Pipeline Behaviors com logging + OpenTelemetry |
| Fail Fast em dados inválidos | ValidationBehavior antes do Handler |
| Desacoplamento de serviços | Eventos via RabbitMQ/Kafka |
| Testabilidade | Interfaces + Testcontainers |
| Observabilidade | Jaeger + Prometheus + Grafana |

---

## Stack deste serviço

- **.NET 8** — API principal
- **PostgreSQL** — persistência
- **Redis** — cache
- **RabbitMQ** — publicação de eventos
- **OpenTelemetry** — traces, métricas e logs
- **Docker Compose** — orquestração local
