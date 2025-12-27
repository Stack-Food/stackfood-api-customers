using Microsoft.EntityFrameworkCore;
using StackFood.Customers.Domain.Entities;
using StackFood.Customers.Infrastructure.Persistence.Configurations;

namespace StackFood.Customers.Infrastructure.Persistence;

public class CustomersDbContext : DbContext
{
    public CustomersDbContext(DbContextOptions<CustomersDbContext> options) : base(options)
    {
    }

    public DbSet<Customer> Customers => Set<Customer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new CustomerConfiguration());
    }
}
