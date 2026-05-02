using ArgenCash.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArgenCash.Infrastructure.Configurations;

public class AssistantPreferencesConfiguration : IEntityTypeConfiguration<AssistantPreferences>
{
    public void Configure(EntityTypeBuilder<AssistantPreferences> builder)
    {
        builder.HasKey(preferences => preferences.Id);

        builder.Property(preferences => preferences.CreatedAtUtc)
            .IsRequired();

        builder.Property(preferences => preferences.UpdatedAtUtc)
            .IsRequired();

        builder.HasIndex(preferences => preferences.UserId)
            .IsUnique();

        builder.HasIndex(preferences => preferences.DefaultExpenseAccountId);
        builder.HasIndex(preferences => preferences.DefaultIncomeAccountId);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(preferences => preferences.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Account>()
            .WithMany()
            .HasForeignKey(preferences => preferences.DefaultExpenseAccountId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne<Account>()
            .WithMany()
            .HasForeignKey(preferences => preferences.DefaultIncomeAccountId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
