using Microsoft.EntityFrameworkCore;
using TransactionApi.Domain.Entities;
using TransactionApi.Domain.Interfaces;
using TransactionApi.Infrastructure.Persistence;

namespace TransactionApi.Infrastructure.Persistence.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly AppDbContext _context;

    public TransactionRepository(AppDbContext context) => _context = context;

    public async Task AddAsync(Transaction transaction, CancellationToken cancellationToken = default)
        => await _context.Transactions.AddAsync(transaction, cancellationToken);

    public Task<Transaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _context.Transactions.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);
}
