# Fluxo PIX Completo

## Visão macro

```
┌─────────────────────────────────────────────────────────────────────┐
│                        FLUXO PIX COMPLETO                           │
│                                                                     │
│  [Cliente Mobile / Web]                                             │
│         │                                                           │
│         ▼                                                           │
│  ┌──────────────┐   auth   ┌──────────────┐                        │
│  │  API Gateway  │────────▶│  Auth Service│                        │
│  │  (Kong/NGINX) │         │  JWT + Redis  │                        │
│  └──────┬────────┘         └──────────────┘                        │
│         │                                                           │
│         ▼                                                           │
│  ┌──────────────────┐  consulta  ┌──────────────────────┐          │
│  │  Transaction API  │──────────▶│  Key Manager Service │          │
│  │  (.NET 8)         │           │  (chaves PIX / DICT)  │          │
│  └────────┬──────────┘           └──────────────────────┘          │
│           │ TransactionCreatedEvent                                 │
│           ▼                                                         │
│     [RabbitMQ]                                                      │
│           │                                                         │
│     ┌─────┴──────────────────────────┐                             │
│     ▼                                ▼                             │
│  ┌──────────────────┐     ┌──────────────────────┐                 │
│  │  Fraud Detection  │     │  Ledger Service       │                │
│  │  Python + Kafka   │     │  .NET + Event Sourcing│                │
│  └──────────────────┘     └──────────┬────────────┘                │
│                                       │                             │
│                           ┌───────────┴──────────┐                 │
│                           ▼                      ▼                 │
│                  ┌────────────────┐  ┌────────────────────┐        │
│                  │  Notification  │  │  BACEN Connector   │        │
│                  │  Node.js       │  │  .NET + Polly       │        │
│                  └────────────────┘  └────────────────────┘        │
└─────────────────────────────────────────────────────────────────────┘
```

---

## Fluxo passo a passo — "Fulano envia R$100 para Ciclano"

```
1. Cliente envia POST /transactions
        │
2. API Gateway verifica rate limit (ex: 10 req/s por CPF)
        │
3. Auth Service valida JWT — retorna claims do usuário
        │
4. Transaction API recebe o comando
        │
        ├── ValidationBehavior: valida campos (amount, currency, accounts)
        │       └── FALHA? → HTTP 400 imediato. Fim.
        │
        ├── Key Manager: resolve chave PIX de destino → accountId real
        │       └── NÃO EXISTE? → HTTP 404. Fim.
        │
        ├── Persiste Transaction com status=PENDING no PostgreSQL
        │
        └── Publica TransactionCreatedEvent no RabbitMQ
                │
5. Fraud Detection consome o evento
        │
        ├── Score < threshold? → emite TransactionApproved
        └── Score >= threshold? → emite TransactionFlagged → bloqueia
                │
6. Ledger Service consome TransactionApproved
        │
        ├── Emite AccountDebited  { acc-001, -100 }  → Kafka
        ├── Emite AccountCredited { acc-002, +100 }  → Kafka
        └── Projeta saldo atualizado
                │
7. BACEN Connector comunica SPI (Sistema de Pagamentos Instantâneos)
        │
        └── Confirmação do BACEN → emite TransactionCompleted
                │
8. Notification Service consome TransactionCompleted
        ├── Push notification para Fulano: "Pagamento enviado ✅"
        └── Push notification para Ciclano: "Você recebeu R$100 ✅"
```

---

## Garantias do fluxo

| Garantia | Mecanismo |
|----------|----------|
| Idempotência | Chave única por transação — retry não cria duplicata |
| Auditoria completa | Event Sourcing no Ledger — cada centavo rastreável |
| Resiliência | Circuit Breaker (Polly) no BACEN Connector |
| Ordem de eventos | Kafka particionado por `accountId` |
| Rastreabilidade distribuída | TraceId propagado via OpenTelemetry em todos os serviços |
| Sem perda de mensagem | RabbitMQ com `durable: true` + Kafka com retenção configurada |
