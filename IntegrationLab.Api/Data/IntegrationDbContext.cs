using IntegrationLab.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace IntegrationLab.Api.Data;

public class IntegrationDbContext : DbContext
{
    public IntegrationDbContext(
        DbContextOptions<IntegrationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Transaction> Transactions => Set<Transaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(t => t.Id);

            entity.HasIndex(t => t.ProviderTransactionId)
                .IsUnique();

            entity.Property(t => t.Amount)
                .HasPrecision(18, 2);

            entity.Property(t => t.ProviderTransactionId)
                .HasMaxLength(100)
                .IsRequired();
        });
    }
}
