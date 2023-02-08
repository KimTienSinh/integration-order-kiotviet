using System;
using System.Collections.Generic;
using Kps.Integration.Api.Models.OrdersKiotViet;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Kps.Integration.Api.Data
{
    public partial class integrationproddbContext : DbContext
    {
        public integrationproddbContext()
        {
        }

        public integrationproddbContext(DbContextOptions<integrationproddbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<KpsOrderKiotviet> KpsOrderKiotviets { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseMySql("name=IntegrationDB", ServerVersion.Parse("8.0.18-mysql"));
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.UseCollation("utf8mb4_0900_ai_ci")
                .HasCharSet("utf8mb4");

            modelBuilder.Entity<KpsOrderKiotviet>(entity =>
            {
                entity.HasKey(e => e.IdKps)
                    .HasName("PRIMARY");

                entity.ToTable("kps_order_kiotviet");

                entity.Property(e => e.IdKps)
                    .HasColumnType("int(11) unsigned")
                    .HasColumnName("idKps");

                entity.Property(e => e.Code)
                    .HasMaxLength(45)
                    .HasColumnName("code");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("timestamp")
                    .HasColumnName("createdAt");

                entity.Property(e => e.CreatedDate)
                    .HasColumnType("timestamp")
                    .HasColumnName("createdDate");

                entity.Property(e => e.CustomerCode)
                    .HasMaxLength(45)
                    .HasColumnName("customerCode");

                entity.Property(e => e.CustomerName)
                    .HasMaxLength(45)
                    .HasColumnName("customerName");

                entity.Property(e => e.Id)
                    .HasColumnType("bigint(20)")
                    .HasColumnName("id")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.SoldByName)
                    .HasMaxLength(45)
                    .HasColumnName("soldByName");

                entity.Property(e => e.Total)
                    .HasColumnName("total")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.TotalPayment)
                    .HasColumnName("totalPayment")
                    .HasDefaultValueSql("'0'");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
