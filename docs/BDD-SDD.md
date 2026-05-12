# Transaction API — BDD + SDD

> **Stack:** .NET 8 · PostgreSQL · Redis · RabbitMQ · OpenTelemetry · Docker Compose  
> **Padrões:** Clean Architecture · CQRS · MediatR Pipeline Behaviors · FluentValidation · xUnit + Testcontainers

---

## Parte 1 — BDD (Behavior-Driven Design)

O BDD define **o que o sistema faz** na perspectiva do usuário, usando linguagem natural (Gherkin-style). Serve de contrato entre negócio e engenharia antes de escrever qualquer linha de código.

---

### Épico: Processamento de Transações Financeiras

Uma conta pode enviar ou receber uma transação. O sistema deve validar, persistir, publicar um evento e retornar o resultado com rastreabilidade completa.

---

### Feature: Criar Transação

#### Cenário 1 — Transação criada com sucesso

```gherkin
Dado que existe uma conta de origem com saldo suficiente
E existe uma conta de destino válida
Quando eu envio POST /transactions com:
  | campo         | valor         |
  | sourceId      | "acc-001"     |
  | destinationId | "acc-002"     |
  | amount        | 150.00        |
  | currency      | "BRL"         |
  | description   | "Pagamento"   |
Então o sistema retorna HTTP 201
E o corpo contém um campo "transactionId" (UUID)
E o campo "status" é "PENDING"
E um evento "TransactionCreated" é publicado no RabbitMQ
```

#### Cenário 2 — Saldo insuficiente

```gherkin
Dado que existe uma conta de origem com saldo de 50.00 BRL
Quando eu envio POST /transactions com amount: 200.00
Então o sistema retorna HTTP 422
E o corpo contém "errors[0].code": "INSUFFICIENT_FUNDS"
E nenhum evento é publicado
```

#### Cenário 3 — Campos obrigatórios ausentes

```gherkin
Quando eu envio POST /transactions sem o campo "amount"
Então o sistema retorna HTTP 400
E o corpo contém "errors[0].field": "amount"
E o campo "errors[0].message" contém "required"
```

#### Cenário 4 — Conta de origem inexistente

```gherkin
Dado que o accountId "acc-999" não existe no sistema
Quando eu envio POST /transactions com sourceId: "acc-999"
Então o sistema retorna HTTP 404
E o corpo contém "errors[0].code": "ACCOUNT_NOT_FOUND"
```

---

### Feature: Consultar Transação

#### Cenário 5 — Consulta por ID existente

```gherkin
Dado que existe uma transação com id "txn-abc"
Quando eu envio GET /transactions/txn-abc
Então o sistema retorna HTTP 200
E o corpo contém todos os campos da transação
```

#### Cenário 6 — Consulta por ID inexistente

```gherkin
Quando eu envio GET /transactions/txn-xyz-desconhecido
Então o sistema retorna HTTP 404
```

---

### Feature: Listar Transações de uma Conta

#### Cenário 7 — Listagem paginada

```gherkin
Dado que a conta "acc-001" tem 25 transações
Quando eu envio GET /accounts/acc-001/transactions?page=1&pageSize=10
Então o sistema retorna HTTP 200
E o corpo contém exatamente 10 itens
E o campo "pagination.totalCount" é 25
E o campo "pagination.hasNextPage" é true
```

---

## Parte 2 — SDD (Software Design Document)

### Visão Geral da Arquitetura

```
HTTP Request
    │
    ▼
┌─────────────────────┐
│     API Layer        │  Controllers, Middleware
└──────────┬──────────┘
           │  IMediator.Send(command/query)
┌──────────▼──────────────────────────┐
│     MediatR Pipeline (em ordem)      │
│  1. LoggingBehavior                  │
│  2. ValidationBehavior (Fail Fast)   │
│  3. PerformanceBehavior              │
│  4. Handler                          │
└──────────┬──────────────────────────┘
           │
┌──────────▼──────────┐
│  Application Layer   │  Commands, Queries
└──────────┬──────────┘
           │
┌──────────▼──────────┐
│   Domain Layer       │  Entities, Events
└──────────┬──────────┘
           │
┌──────────▼──────────┐
│ Infrastructure Layer │  DB, Cache, MQ
└─────────────────────┘
```

### Fluxo Completo de uma Requisição

```
1. POST /transactions
   └── TransactionsController.CreateAsync()
       └── _mediator.Send(CreateTransactionCommand)
           │
           ├── LoggingBehavior         → loga início com correlationId
           ├── ValidationBehavior      → roda CreateTransactionValidator
           │   └── FAIL? → lança ValidationException → ExceptionMiddleware → HTTP 400
           ├── PerformanceBehavior     → inicia stopwatch
           │
           └── CreateTransactionCommandHandler
               ├── AccountService.GetByIdAsync()      → valida conta origem
               ├── AccountService.ValidateDestinationAsync()
               ├── (FAIL?) → lança InsufficientFundsException → HTTP 422
               ├── Transaction.Create()               → cria entidade
               ├── TransactionRepository.AddAsync()   → persiste no PostgreSQL
               ├── EventPublisher.PublishAsync()       → publica no RabbitMQ
               └── retorna CreateTransactionResult
           │
           ├── PerformanceBehavior     → para stopwatch, alerta se > 500ms
           └── LoggingBehavior         → loga conclusão com duração
```
