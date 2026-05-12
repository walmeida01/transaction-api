using MediatR;
using TransactionApi.Domain.Entities;
using TransactionApi.Domain.Interfaces;

namespace TransactionApi.Application.Transactions.Queries.GetTransaction;

public class GetTransactionQueryHandler
    : IRequestHandler<GetTransactionQuery, Transaction?>
{
    private readonly ITransactionRepository _repository;

    public GetTransactionQueryHandler(ITransactionRepository repository)
        => _repository = repository;

    public Task<Transaction?> Handle(
        GetTransactionQuery request,
        CancellationToken cancellationToken)
        => _repository.GetByIdAsync(request.TransactionId, cancellationToken);
}
