using ArgenCash.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArgenCash.Infrastructure.Configurations
{
    public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
    {
        public void Configure(EntityTypeBuilder<Transaction> builder)
        {
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Description)
                .IsRequired()
                .HasMaxLength(250);

            builder.Property(t => t.Amount)
                .IsRequired()
                .HasPrecision(19, 4);

            builder.Property(t => t.TransactionType)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(t => t.ConvertedAmountUSD)
                .IsRequired()
                .HasPrecision(19, 4);

            builder.Property(t => t.ConvertedAmountARS)
                .IsRequired()
                .HasPrecision(19, 4);

            builder.Property(t => t.Currency)
                .IsRequired()
                .HasMaxLength(3);

            builder.Property(t => t.TransactionDate)
                .IsRequired();

            builder.HasOne<Account>().WithMany()
                .HasForeignKey(t => t.AccountId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<ExchangeRate>()
                .WithMany()
                .HasForeignKey(t => t.ExchangeRateId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(t => t.ExchangeRateId);

            builder.ToTable(table =>
            {
                table.HasCheckConstraint("CK_Transactions_Amount_Positive", "\"Amount\" > 0");
                table.HasCheckConstraint("CK_Transactions_ConvertedAmountUSD_Positive", "\"ConvertedAmountUSD\" > 0");
                table.HasCheckConstraint("CK_Transactions_ConvertedAmountARS_Positive", "\"ConvertedAmountARS\" > 0");
                table.HasCheckConstraint("CK_Transactions_Currency_Length", "char_length(\"Currency\") = 3");
                table.HasCheckConstraint("CK_Transactions_Type_Allowed", "\"TransactionType\" IN ('INCOME', 'EXPENSE')");
            });
        }
    }
}
