using ConcurrencyLab.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ConcurrencyLab.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<AppUser> Users => Set<AppUser>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>()
            .Property(product => product.Version)
            .IsConcurrencyToken();

        modelBuilder.Entity<AppUser>()
            .HasIndex(user => user.Username)
            .IsUnique();
    }
}
