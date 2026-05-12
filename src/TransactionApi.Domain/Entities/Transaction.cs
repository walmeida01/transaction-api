namespace TransactionApi.Domain.Entities;

public class Transaction
{
    public Guid Id { get; private set; }
    public string SourceAccountId { get; private set; } = default!;
    public string DestinationAccountId { get; private set; } = default!;
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = default!;
    public TransactionStatus Status { get; private set; }
    public string? Description { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private Transaction() { } // EF Core

    public static Transaction Create(
        string sourceAccountId,
        string destinationAccountId,
        decimal amount,
        string currency,
        string? description = null)
    {
        if (amount <= 0)
            throw new DomainException("Amount must be greater than zero.");

        return new Transaction
        {
            Id = Guid.NewGuid(),
            SourceAccountId = sourceAccountId,
            DestinationAccountId = destinationAccountId,
            Amount = amount,
            Currency = currency.ToUpperInvariant(),
            Status = TransactionStatus.Pending,
            Description = description,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Complete()
    {
        Status = TransactionStatus.Completed;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Fail()
    {
        Status = TransactionStatus.Failed;
        UpdatedAt = DateTime.UtcNow;
    }
}
