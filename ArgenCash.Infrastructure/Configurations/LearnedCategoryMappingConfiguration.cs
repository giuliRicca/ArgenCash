using ArgenCash.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ArgenCash.Infrastructure.Configurations;

public class LearnedCategoryMappingConfiguration : IEntityTypeConfiguration<LearnedCategoryMapping>
{
    public void Configure(EntityTypeBuilder<LearnedCategoryMapping> builder)
    {
        builder.HasKey(mapping => mapping.Id);

        builder.Property(mapping => mapping.NormalizedKey)
            .IsRequired()
            .HasMaxLength(120);

        builder.Property(mapping => mapping.TransactionType)
            .IsRequired()
            .HasConversion(new ValueConverter<TransactionType, string>(
                v => TransactionTypes.ToString(v),
                v => TransactionTypes.ToEnum(v)))
            .HasMaxLength(20);

        builder.Property(mapping => mapping.CreatedAtUtc)
            .IsRequired();

        builder.Property(mapping => mapping.UpdatedAtUtc)
            .IsRequired();

        builder.HasIndex(mapping => new { mapping.UserId, mapping.NormalizedKey, mapping.TransactionType })
            .IsUnique();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(mapping => mapping.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Category>()
            .WithMany()
            .HasForeignKey(mapping => mapping.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
