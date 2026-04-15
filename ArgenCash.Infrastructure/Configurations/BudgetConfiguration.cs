using ArgenCash.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArgenCash.Infrastructure.Configurations;

public class BudgetConfiguration : IEntityTypeConfiguration<Budget>
{
    public void Configure(EntityTypeBuilder<Budget> builder)
    {
        builder.HasKey(budget => budget.Id);

        builder.Property(budget => budget.Amount)
            .IsRequired()
            .HasPrecision(19, 2);

        builder.Property(budget => budget.Currency)
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(budget => budget.CreatedAtUtc)
            .IsRequired();

        builder.Property(budget => budget.UpdatedAtUtc)
            .IsRequired();

        builder.HasIndex(budget => new { budget.UserId, budget.CategoryId })
            .IsUnique();

        builder.HasIndex(budget => budget.UserId);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(budget => budget.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Category>()
            .WithMany()
            .HasForeignKey(budget => budget.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable(table =>
        {
            table.HasCheckConstraint("CK_Budgets_Amount_Positive", "\"Amount\" > 0");
            table.HasCheckConstraint("CK_Budgets_Currency_Length", "char_length(\"Currency\") = 3");
            table.HasCheckConstraint("CK_Budgets_Currency_Allowed", "\"Currency\" IN ('USD', 'ARS')");
        });
    }
}
