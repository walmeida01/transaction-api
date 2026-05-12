# transaction-api

API de processamento de transações financeiras em .NET 8, seguindo Clean Architecture, CQRS, MediatR Pipeline Behaviors, PostgreSQL, Redis, RabbitMQ e OpenTelemetry.

## Stack

- **.NET 8** — framework principal
- **PostgreSQL** — banco de dados relacional
- **Redis** — cache
- **RabbitMQ** — mensageria / eventos de domínio
- **OpenTelemetry + Jaeger + Prometheus + Grafana** — observabilidade
- **Docker Compose** — orquestração de infraestrutura

## Padrões utilizados

- Clean Architecture
- CQRS com MediatR
- Pipeline Behaviors (Logging, Validation Fail Fast, Performance)
- FluentValidation
- Testes unitários com xUnit + Moq
- Testes de integração com Testcontainers

## Como rodar

```bash
# Subir infraestrutura
docker compose up -d

# Aplicar migrations
dotnet ef database update --project src/TransactionApi.Infrastructure --startup-project src/TransactionApi.Api

# Rodar a API
dotnet run --project src/TransactionApi.Api

# Testes unitários
dotnet test tests/TransactionApi.UnitTests

# Testes de integração (requer Docker)
dotnet test tests/TransactionApi.IntegrationTests
```

## Serviços disponíveis

| Serviço        | URL                       |
|----------------|---------------------------|
| API            | http://localhost:5000     |
| RabbitMQ UI    | http://localhost:15672    |
| Jaeger         | http://localhost:16686    |
| Prometheus     | http://localhost:9090     |
| Grafana        | http://localhost:3000     |

## Documentação

Consulte [`docs/BDD-SDD.md`](docs/BDD-SDD.md) para o documento completo de BDD + SDD.
