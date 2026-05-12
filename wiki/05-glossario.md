# Glossário

## Termos de Domínio

| Termo | Definição |
|-------|-----------|
| **PIX** | Sistema de pagamentos instantâneos brasileiro, operado pelo Banco Central (BACEN) |
| **Chave PIX** | Identificador de uma conta: CPF, email, telefone ou chave aleatória |
| **DICT** | Diretório de Identificadores de Contas Transacionais — base de dados de chaves PIX do BACEN |
| **SPI** | Sistema de Pagamentos Instantâneos — infraestrutura do BACEN que processa as transações PIX |
| **EndToEndId** | Identificador único de uma transação PIX no SPI — garante idempotência |
| **Liquidação** | Processo de débito na conta de origem e crédito na conta de destino |
| **PSP** | Provedor de Serviços de Pagamento — banco ou fintech participante do PIX |

---

## Termos Arquiteturais

| Termo | Definição |
|-------|-----------|
| **Clean Architecture** | Padrão que organiza o código em camadas com dependências apontando para o domínio |
| **CQRS** | Command Query Responsibility Segregation — separação de operações de leitura e escrita |
| **MediatR** | Biblioteca .NET que implementa o padrão Mediator — desacopla quem envia de quem processa |
| **Pipeline Behavior** | Interceptor do MediatR executado antes/depois de cada handler |
| **Fail Fast** | Estratégia de falhar o mais cedo possível — o ValidationBehavior impede handlers com dados inválidos |
| **Event Sourcing** | Padrão onde o estado é derivado de uma sequência imutável de eventos |
| **Event Driven** | Arquitetura onde serviços se comunicam publicando e consumindo eventos assíncronos |
| **ADR** | Architecture Decision Record — documento que registra uma decisão arquitetural e seu contexto |
| **Idempotência** | Propriedade de uma operação que pode ser executada múltiplas vezes com o mesmo resultado |
| **Circuit Breaker** | Padrão que interrompe chamadas a um serviço com falha para evitar sobrecarga em cascata |
| **Backoff Exponencial** | Estratégia de retry onde o tempo de espera dobra a cada tentativa |
| **TraceId** | Identificador único que percorre todos os serviços de uma requisição distribuída |

---

## Termos de Infraestrutura

| Termo | Definição |
|-------|-----------|
| **RabbitMQ** | Message broker baseado em push — ideal para comandos pontuais com entrega garantida |
| **Kafka** | Plataforma de streaming distribuído baseada em log imutável — ideal para auditoria e replay |
| **Exchange (RabbitMQ)** | Roteador de mensagens — recebe publicações e as distribui para filas baseado em routing key |
| **Topic Exchange** | Tipo de exchange que roteia mensagens por padrão (ex: `transaction.created`) |
| **Partition (Kafka)** | Subdivisão de um tópico Kafka — eventos da mesma partição são ordenados |
| **Consumer Group (Kafka)** | Grupo de consumers que divide o processamento de um tópico entre si |
| **Testcontainers** | Biblioteca que sobe containers Docker reais dentro de testes automatizados |
| **OpenTelemetry** | Padrão open-source para coleta de traces, métricas e logs em sistemas distribuídos |
| **Jaeger** | Ferramenta de distributed tracing — visualiza o caminho de uma requisição entre serviços |
| **Prometheus** | Banco de dados de séries temporais para métricas — coleta e armazena métricas dos serviços |
| **Grafana** | Plataforma de visualização — cria dashboards a partir de Prometheus, Jaeger e outros |
| **Polly** | Biblioteca .NET de resiliência — retry, circuit breaker, timeout, bulkhead |
| **Healthcheck** | Endpoint que indica se um serviço está operacional — usado pelo Docker Compose e orquestradores |
