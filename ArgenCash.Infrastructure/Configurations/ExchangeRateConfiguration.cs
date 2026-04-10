using ArgenCash.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArgenCash.Infrastructure.Configurations
{
    public class ExchangeRateConfiguration : IEntityTypeConfiguration<ExchangeRate>
    {
        public void Configure(EntityTypeBuilder<ExchangeRate> builder)
        {
            builder.HasKey(a => a.Id);

            builder.Property(a => a.RateType)
                .IsRequired()
                .HasConversion(new ValueConverter<ExchangeRateType, string>(
                    value => ExchangeRateTypes.ToString(value),
                    value => ExchangeRateTypes.ToEnum(value)))
                .HasMaxLength(20);
            builder.Property(a => a.BaseCurrency).IsRequired().HasMaxLength(3);
            builder.Property(a => a.TargetCurrency).IsRequired().HasMaxLength(3);
            builder.Property(a => a.BuyPrice).IsRequired().HasPrecision(19, 4);
            builder.Property(a => a.SellPrice).IsRequired().HasPrecision(19, 4);
            builder.Property(a => a.EffectiveDate).IsRequired();

            builder.HasIndex(a => new { a.BaseCurrency, a.TargetCurrency, a.RateType, a.EffectiveDate });

            builder.ToTable(table =>
            {
                table.HasCheckConstraint("CK_ExchangeRates_BaseCurrency_Length", "char_length(\"BaseCurrency\") = 3");
                table.HasCheckConstraint("CK_ExchangeRates_TargetCurrency_Length", "char_length(\"TargetCurrency\") = 3");
                table.HasCheckConstraint("CK_ExchangeRates_PositivePrices", "\"BuyPrice\" > 0 AND \"SellPrice\" > 0");
                table.HasCheckConstraint("CK_ExchangeRates_BuyPrice_Lte_SellPrice", "\"BuyPrice\" <= \"SellPrice\"");
                table.HasCheckConstraint("CK_ExchangeRates_DifferentCurrencies", "\"BaseCurrency\" <> \"TargetCurrency\"");
            });
        }
    }
}
