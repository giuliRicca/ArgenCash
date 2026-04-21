using ArgenCash.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArgenCash.Infrastructure.Configurations;

/// <summary>
/// Configures persistence mapping for <see cref="CreditCardStatementSettlement"/>.
/// </summary>
public class CreditCardStatementSettlementConfiguration : IEntityTypeConfiguration<CreditCardStatementSettlement>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<CreditCardStatementSettlement> builder)
    {
        builder.HasKey(settlement => settlement.Id);

        builder.Property(settlement => settlement.UserId)
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(settlement => settlement.CreditAccountId)
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(settlement => settlement.FundingAccountId)
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(settlement => settlement.StatementYear)
            .IsRequired();

        builder.Property(settlement => settlement.StatementMonth)
            .IsRequired();

        builder.Property(settlement => settlement.Amount)
            .IsRequired()
            .HasPrecision(19, 4);

        builder.Property(settlement => settlement.Currency)
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(settlement => settlement.TransferGroupId)
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(settlement => settlement.CreatedAtUtc)
            .IsRequired();

        builder.HasIndex(settlement => new { settlement.CreditAccountId, settlement.StatementYear, settlement.StatementMonth })
            .IsUnique();

        builder.HasIndex(settlement => settlement.UserId);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(settlement => settlement.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Account>()
            .WithMany()
            .HasForeignKey(settlement => settlement.CreditAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Account>()
            .WithMany()
            .HasForeignKey(settlement => settlement.FundingAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable(table =>
        {
            table.HasCheckConstraint("CK_CreditCardStatementSettlements_Amount_Positive", "\"Amount\" > 0");
            table.HasCheckConstraint("CK_CreditCardStatementSettlements_Currency_Length", "char_length(\"Currency\") = 3");
            table.HasCheckConstraint("CK_CreditCardStatementSettlements_StatementMonth_Range", "\"StatementMonth\" BETWEEN 1 AND 12");
        });
    }
}
