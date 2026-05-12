# Decisões Arquiteturais (ADRs)

> ADR = *Architecture Decision Record* — registro do raciocínio por trás de cada escolha técnica relevante.

---

## ADR-001 — Clean Architecture

**Contexto:** Sistemas financeiros mudam de banco de dados, de mensageria e de frameworks com frequência. Regras de negócio não podem ser refatoradas junto com infraestrutura.

**Decisão:** Separar o projeto em quatro camadas com dependências apontando sempre para dentro:

```
Api → Application → Domain
Infrastructure → Application → Domain
```

**Consequências:**
- ✅ Trocar PostgreSQL por DynamoDB exige mudança apenas em `Infrastructure`
- ✅ Testes unitários do domínio não precisam de banco ou container
- ⚠️ Mais arquivos e indireção — curva de aprendizado inicial maior

---

## ADR-002 — CQRS com MediatR

**Contexto:** Operações de leitura e escrita têm requisitos diferentes — escrita precisa de validação pesada e publicação de eventos; leitura precisa de performance e pode usar cache.

**Decisão:** Separar Commands (escrita) de Queries (leitura) usando o padrão CQRS via MediatR.

**Consequências:**
- ✅ Cada operação tem seu próprio handler, validator e pipeline
- ✅ Queries podem evoluir para bancos de leitura otimizados (ex: Elasticsearch) sem afetar Commands
- ⚠️ Mais classes — cada operação tem Command/Query + Handler + Validator

---

## ADR-003 — MediatR Pipeline Behaviors

**Contexto:** Logging, validação e métricas precisam ser aplicados em toda operação, sem depender de cada dev lembrar de chamar.

**Decisão:** Registrar behaviors em ordem no pipeline do MediatR:

```
Requisição → LoggingBehavior → ValidationBehavior → PerformanceBehavior → Handler
```

**Por que essa ordem?**
- `Logging` primeiro: registra até requisições que falham na validação
- `Validation` antes do Handler: **Fail Fast** — handler nunca executa com dados inválidos
- `Performance` antes do Handler: mede apenas o tempo de execução real do handler

**Consequências:**
- ✅ Garantia de que nenhum handler escapa do pipeline
- ✅ Novos comportamentos transversais (ex: caching, idempotência) adicionados em um só lugar
- ⚠️ Ordem de registro importa — erro na ordem quebra o comportamento esperado

---

## ADR-004 — Eventos de Domínio via RabbitMQ

**Contexto:** Após criar a transação, outros serviços precisam reagir (ledger, fraude, notificação). Chamar esses serviços de forma síncrona criaria acoplamento e fragilidade.

**Decisão:** Publicar `TransactionCreatedEvent` no RabbitMQ com exchange do tipo `topic`.

**Por que RabbitMQ e não Kafka aqui?**

| Critério | RabbitMQ | Kafka |
|----------|----------|-------|
| Entrega garantida por mensagem | ✅ | ✅ |
| Replay de eventos históricos | ❌ | ✅ |
| Retenção longa (auditoria) | ❌ | ✅ |
| Simplicidade operacional | ✅ | ⚠️ |
| Ideal para | Comandos pontuais | Streaming / auditoria |

RabbitMQ é suficiente para publicar o evento de criação. O Ledger Service (que precisa de replay e auditoria) usa Kafka internamente.

---

## ADR-005 — Testcontainers nos Testes de Integração

**Contexto:** Mockar banco de dados nos testes de integração esconde comportamentos reais (constraints, transações, índices).

**Decisão:** Usar Testcontainers para subir PostgreSQL real em Docker durante os testes.

**Consequências:**
- ✅ Ambiente de teste idêntico ao de produção
- ✅ Testes detectam erros de migration e constraint
- ⚠️ Testes mais lentos (docker pull na primeira execução) e requerem Docker no CI

---

## ADR-006 — ExceptionHandlingMiddleware centralizado

**Contexto:** Sem tratamento centralizado, cada controller ou handler precisaria capturar e formatar erros individualmente — inconsistência garantida.

**Decisão:** Um único middleware captura todas as exceções e as traduz para respostas HTTP padronizadas:

| Exceção | HTTP Status | Código |
|---------|-------------|--------|
| `ValidationException` | 400 | `VALIDATION_ERROR` |
| `AccountNotFoundException` | 404 | `ACCOUNT_NOT_FOUND` |
| `InsufficientFundsException` | 422 | `INSUFFICIENT_FUNDS` |
| `Exception` (qualquer outra) | 500 | `SERVER_ERROR` |

**Consequências:**
- ✅ Resposta de erro sempre no mesmo formato
- ✅ Handlers lançam exceções sem se preocupar com HTTP
- ✅ Fácil de adicionar novos mapeamentos
