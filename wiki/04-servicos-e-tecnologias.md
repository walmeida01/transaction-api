# Serviços e Tecnologias

## Mapa de decisões tecnológicas

| Serviço | Tecnologia | Por que essa escolha |
|---------|-----------|---------------------|
| API Gateway | Kong ou NGINX | Rate limiting, auth centralizado, roteamento sem tocar nos serviços |
| Auth Service | Go + Redis | Latência crítica — Go tem P99 menor que .NET para esse workload; Redis para revogação de tokens |
| Transaction API | .NET 8 | Ecossistema rico (.NET para APIs REST), MediatR, FluentValidation, EF Core |
| Key Manager | .NET 8 + PostgreSQL | CRUD de chaves PIX, integração com DICT do BACEN |
| Ledger Service | .NET 8 + Kafka + Event Sourcing | Auditoria regulatória exige histórico imutável; Kafka para replay e retenção |
| Fraud Detection | Python + Kafka Streams | Ecossistema de ML é Python; Kafka Streams para análise em tempo real |
| BACEN Connector | .NET 8 + Polly | Resiliência (retry, circuit breaker) na integração com SPI |
| Notification Service | Node.js | I/O intensivo — envio de push/SMS/email; modelo assíncrono nativo |

---

## API Gateway

**Responsabilidade:** Ponto único de entrada. Nenhum serviço é exposto diretamente à internet.

**Funcionalidades:**
- Rate limiting por CPF/IP (ex: 10 transações/segundo)
- Autenticação JWT centralizada
- Roteamento para microsserviços
- Circuit breaker global
- Logs de acesso unificados

**Alternativas consideradas:** AWS API Gateway (vendor lock-in), Traefik (menos recursos de negócio).

---

## Auth Service

**Responsabilidade:** Emitir e validar tokens JWT. Gerenciar sessões e revogação.

**Por que Go e não .NET?**
Autenticação é chamada em cada requisição. Go tem startup em milissegundos e throughput de autenticação ~3x maior que .NET para esse caso específico. Para lógica de negócio complexa, .NET vence — para I/O puro e baixa latência, Go vence.

**Redis:** Armazena tokens com TTL para revogação instantânea. JWT puro não permite revogar um token antes do vencimento — Redis resolve isso.

---

## Ledger Service — A decisão mais importante

**Responsabilidade:** Débito e crédito de contas. Cálculo de saldo. Auditoria.

### Por que Event Sourcing?

Modelo tradicional:
```sql
UPDATE accounts SET balance = balance - 100 WHERE id = 'acc-001';
```
Problema: você sabe o saldo atual, mas **não sabe como chegou lá**.

Event Sourcing:
```
AccountDebited  { id: acc-001, amount: 100, txn: txn-abc, at: 2026-05-12T03:00:00Z }
AccountCredited { id: acc-002, amount: 100, txn: txn-abc, at: 2026-05-12T03:00:00Z }
```
O saldo é a **projeção** de todos os eventos. Você pode:
- Recalcular o saldo em qualquer ponto do tempo
- Fazer replay para corrigir bugs
- Auditar cada centavo (obrigatório pelo BACEN)
- Reverter uma transação emitindo evento inverso

### Por que Kafka no Ledger (e não RabbitMQ)?

```
RabbitMQ: mensagem entregue → consumida → deletada
Kafka:    mensagem entregue → consumida → RETIDA por N dias
```

O Ledger precisa de:
- **Retenção**: manter eventos por anos (auditoria regulatória)
- **Replay**: reprocessar eventos históricos se houver bug
- **Ordering**: eventos da mesma conta processados em ordem (particionamento por accountId)
- **Throughput**: Kafka escala para milhões de eventos/segundo

---

## Fraud Detection

**Responsabilidade:** Analisar padrões e sinalizar transações suspeitas.

**Modelo assíncrono e consultivo:** O serviço não bloqueia o fluxo. Ele analisa o evento e emite `TransactionApproved` ou `TransactionFlagged`. Isso mantém a latência do PIX < 10 segundos (exigência do BACEN).

**Features do modelo ML:**
- Valor fora do padrão histórico do usuário
- Horário incomum
- Frequência de transações por janela de tempo
- Geolocalização inconsistente
- Chave de destino nova / nunca usada

---

## BACEN Connector

**Responsabilidade:** Comunicar o SPI (Sistema de Pagamentos Instantâneos) do Banco Central.

**Polly — Resiliência em camadas:**
```
Retry com backoff exponencial:
  tentativa 1 → aguarda 1s → tentativa 2 → aguarda 2s → tentativa 3...

Circuit Breaker:
  5 falhas consecutivas → circuito ABERTO por 30s
  Circuito aberto → falha fast sem esperar timeout
  Após 30s → circuito HALF-OPEN → testa uma requisição
```

**Idempotência:** Cada transação tem `endToEndId` único. Se o BACEN receber a mesma requisição duas vezes (por retry), ele idempotentemente retorna o mesmo resultado sem processar duas vezes.

---

## Observabilidade — Os três pilares

```
┌─────────────┐  ┌─────────────┐  ┌─────────────┐
│   TRACES    │  │   METRICS   │  │    LOGS     │
│  (Jaeger)   │  │ (Prometheus)│  │  (Grafana)  │
│             │  │             │  │             │
│ Rastreia    │  │ Mede        │  │ Correlaciona│
│ o caminho   │  │ throughput, │  │ tudo pelo   │
│ de uma req  │  │ erros, P99  │  │ TraceId     │
│ entre todos │  │             │  │             │
│ os serviços │  │             │  │             │
└─────────────┘  └─────────────┘  └─────────────┘
        │               │               │
        └───────────────┴───────────────┘
                        │
              [OpenTelemetry Collector]
              ponto único de coleta e
              roteamento de telemetria
```

O `TraceId` do OpenTelemetry é propagado em todos os serviços via header HTTP. Isso permite ver no Jaeger o caminho completo de uma transação: Transaction API → Ledger → Notification — tudo em uma única trace.
