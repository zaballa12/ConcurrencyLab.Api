using ConcurrencyLab.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ConcurrencyLab.Api.Data;
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>()
            .Property(product => product.Version)
            .IsConcurrencyToken();
    }
}
