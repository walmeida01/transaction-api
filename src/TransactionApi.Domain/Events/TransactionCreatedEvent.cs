namespace TransactionApi.Domain.Events;

public record TransactionCreatedEvent(
    Guid TransactionId,
    decimal Amount,
    string Currency);
