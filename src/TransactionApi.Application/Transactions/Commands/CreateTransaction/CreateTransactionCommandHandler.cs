using MediatR;
using Microsoft.Extensions.Logging;
using TransactionApi.Domain.Entities;
using TransactionApi.Domain.Events;
using TransactionApi.Domain.Exceptions;
using TransactionApi.Domain.Interfaces;

namespace TransactionApi.Application.Transactions.Commands.CreateTransaction;

public class CreateTransactionCommandHandler
    : IRequestHandler<CreateTransactionCommand, CreateTransactionResult>
{
    private readonly ITransactionRepository _repository;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<CreateTransactionCommandHandler> _logger;

    public CreateTransactionCommandHandler(
        ITransactionRepository repository,
        IEventPublisher eventPublisher,
        ILogger<CreateTransactionCommandHandler> logger)
    {
        _repository = repository;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task<CreateTransactionResult> Handle(
        CreateTransactionCommand request,
        CancellationToken cancellationToken)
    {
        // TODO: integrar com IAccountService para validar saldo
        // Por ora, criamos a transação diretamente para fins de estudo

        var transaction = Transaction.Create(
            request.SourceAccountId,
            request.DestinationAccountId,
            request.Amount,
            request.Currency,
            request.Description);

        await _repository.AddAsync(transaction, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        await _eventPublisher.PublishAsync(
            new TransactionCreatedEvent(transaction.Id, transaction.Amount, transaction.Currency),
            cancellationToken);

        _logger.LogInformation("Transaction {TransactionId} created successfully.", transaction.Id);

        return new CreateTransactionResult(transaction.Id, transaction.Status.ToString());
    }
}
