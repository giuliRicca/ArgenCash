using ArgenCash.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
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

            builder.Property(a => a.CreatedAt)
                .IsRequired();

            builder.HasIndex(a => a.UserId);

            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.ToTable(table =>
            {
                table.HasCheckConstraint("CK_Accounts_CurrencyCode_Length", "char_length(\"CurrencyCode\") = 3");
            });
        }
    }
}
