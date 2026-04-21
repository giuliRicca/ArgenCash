using ArgenCash.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArgenCash.Infrastructure.Configurations
{
    public class AccountConfiguration : IEntityTypeConfiguration<Account>
    {
        public void Configure(EntityTypeBuilder<Account> builder)
        {
            builder.HasKey(a => a.Id);
            builder.Property(a => a.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(a => a.UserId)
                .HasColumnType("uuid");

            builder.Property(a => a.CurrencyCode)
                .IsRequired()
                .HasMaxLength(3);

            builder.Property(a => a.ExchangeRateType)
                .IsRequired()
                .HasConversion(new ValueConverter<ExchangeRateType, string>(
                    value => ExchangeRateTypes.ToString(value),
                    value => ExchangeRateTypes.ToEnum(value)))
                .HasDefaultValue(ExchangeRateType.Official)
                .HasMaxLength(20);

            builder.Property(a => a.AccountType)
                .IsRequired()
                .HasConversion(new ValueConverter<AccountType, string>(
                    value => AccountTypes.ToString(value),
                    value => AccountTypes.ToEnum(value)))
                .HasDefaultValue(AccountType.Standard)
                .HasMaxLength(20);

            builder.Property(a => a.FundingAccountId)
                .HasColumnType("uuid");

            builder.Property(a => a.PaymentDayOfMonth);

            builder.Property(a => a.CreatedAt)
                .IsRequired();

            builder.HasIndex(a => a.UserId);
            builder.HasIndex(a => a.FundingAccountId);

            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<Account>()
                .WithMany()
                .HasForeignKey(a => a.FundingAccountId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.ToTable(table =>
            {
                table.HasCheckConstraint("CK_Accounts_CurrencyCode_Length", "char_length(\"CurrencyCode\") = 3");
                table.HasCheckConstraint("CK_Accounts_ExchangeRateType_Allowed", "\"ExchangeRateType\" IN ('OFFICIAL', 'CCL', 'MEP', 'BLUE', 'CRYPTO')");
                table.HasCheckConstraint("CK_Accounts_AccountType_Allowed", "\"AccountType\" IN ('STANDARD', 'CREDIT')");
                table.HasCheckConstraint(
                    "CK_Accounts_CreditSettings_Consistency",
                    "(\"AccountType\" = 'CREDIT' AND \"FundingAccountId\" IS NOT NULL AND \"PaymentDayOfMonth\" BETWEEN 1 AND 28) OR (\"AccountType\" = 'STANDARD' AND \"FundingAccountId\" IS NULL AND \"PaymentDayOfMonth\" IS NULL)");
            });
        }
    }
}
