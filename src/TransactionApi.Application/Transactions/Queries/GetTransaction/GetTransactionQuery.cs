using MediatR;
using TransactionApi.Domain.Entities;

namespace TransactionApi.Application.Transactions.Queries.GetTransaction;

public record GetTransactionQuery(Guid TransactionId) : IRequest<Transaction?>;
