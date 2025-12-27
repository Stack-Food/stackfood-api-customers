using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StackFood.Customers.Domain.Entities;

namespace StackFood.Customers.Infrastructure.Persistence.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("customers");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(c => c.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(c => c.Email)
            .HasColumnName("email")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(c => c.Cpf)
            .HasColumnName("cpf")
            .HasMaxLength(14)
            .IsRequired();

        builder.Property(c => c.CognitoUserId)
            .HasColumnName("cognito_user_id")
            .HasMaxLength(100);

        builder.Property(c => c.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(c => c.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        // Indexes
        builder.HasIndex(c => c.Cpf)
            .IsUnique()
            .HasDatabaseName("idx_customers_cpf");

        builder.HasIndex(c => c.Email)
            .IsUnique()
            .HasDatabaseName("idx_customers_email");

        builder.HasIndex(c => c.CognitoUserId)
            .HasDatabaseName("idx_customers_cognito_user_id");
    }
}
