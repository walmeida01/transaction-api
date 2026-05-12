using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using TransactionApi.Domain.Interfaces;

namespace TransactionApi.Infrastructure.Messaging;

public class RabbitMqEventPublisher : IEventPublisher
{
    private readonly IConnection _connection;
    private readonly ILogger<RabbitMqEventPublisher> _logger;
    private const string ExchangeName = "transactions";

    public RabbitMqEventPublisher(IConnection connection, ILogger<RabbitMqEventPublisher> logger)
    {
        _connection = connection;
        _logger = logger;
    }

    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class
    {
        using var channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await channel.ExchangeDeclareAsync(
            exchange: ExchangeName,
            type: ExchangeType.Topic,
            durable: true,
            cancellationToken: cancellationToken);

        var routingKey = typeof(TEvent).Name.ToLower();
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(@event));

        await channel.BasicPublishAsync(
            exchange: ExchangeName,
            routingKey: routingKey,
            body: body,
            cancellationToken: cancellationToken);

        _logger.LogInformation(
            "Event {EventType} published to exchange '{Exchange}' with routing key '{RoutingKey}'",
            typeof(TEvent).Name, ExchangeName, routingKey);
    }
}
