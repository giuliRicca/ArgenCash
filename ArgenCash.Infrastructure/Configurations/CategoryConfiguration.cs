using ArgenCash.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ArgenCash.Infrastructure.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.HasKey(category => category.Id);

        builder.Property(category => category.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(category => category.Type)
            .IsRequired()
            .HasConversion(new ValueConverter<TransactionType, string>(
                v => TransactionTypes.ToString(v),
                v => TransactionTypes.ToEnum(v)))
            .HasMaxLength(20);

        builder.Property(category => category.IsSystem)
            .IsRequired();

        builder.Property(category => category.CreatedAtUtc)
            .IsRequired();

        builder.HasIndex(category => new { category.Name, category.UserId })
            .IsUnique();

        builder.HasIndex(category => category.UserId);

        builder.ToTable("Categories");
    }
}
