using MediatR;

namespace TransactionApi.Application.Transactions.Commands.CreateTransaction;

public record CreateTransactionCommand(
    string SourceAccountId,
    string DestinationAccountId,
    decimal Amount,
    string Currency,
    string? Description = null) : IRequest<CreateTransactionResult>;

public record CreateTransactionResult(
    Guid TransactionId,
    string Status);
