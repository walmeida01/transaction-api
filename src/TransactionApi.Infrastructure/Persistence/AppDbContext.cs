using Microsoft.EntityFrameworkCore;
using TransactionApi.Domain.Entities;

namespace TransactionApi.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Transaction> Transactions => Set<Transaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.SourceAccountId).IsRequired().HasMaxLength(50);
            entity.Property(t => t.DestinationAccountId).IsRequired().HasMaxLength(50);
            entity.Property(t => t.Amount).HasColumnType("decimal(18,2)");
            entity.Property(t => t.Currency).IsRequired().HasMaxLength(3);
            entity.Property(t => t.Status).HasConversion<string>();
            entity.Property(t => t.Description).HasMaxLength(255);
        });
    }
}
