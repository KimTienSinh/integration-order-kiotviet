﻿// <auto-generated />
using System;
using Kps.Integration.Api.Infra;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Kps.Integration.Api.Migrations
{
    [DbContext(typeof(KpsIntegrationContext))]
    [Migration("20220326144614_Update_OrderTable_Update_Nulable_Columns")]
    partial class Update_OrderTable_Update_Nulable_Columns
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("Kps.Integration.Domain.Entities.CustomerMapping", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("CustomerEmail")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<int>("CustomerId")
                        .HasColumnType("int");

                    b.Property<string>("CustomerPhone")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("GetflyCustomerCode")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<int?>("GetflyCustomerId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("CustomerMapping");
                });

            modelBuilder.Entity("Kps.Integration.Domain.Entities.Order", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("datetime(6)");

                    b.Property<int?>("CustomerId")
                        .HasColumnType("int");

                    b.Property<string>("GetflyCustomerCode")
                        .HasColumnType("longtext");

                    b.Property<int?>("GetflyOrderId")
                        .HasColumnType("int");

                    b.Property<byte[]>("GetflyRequestBody")
                        .HasColumnType("longblob");

                    b.Property<DateTime?>("LastRetriedOn")
                        .HasColumnType("datetime(6)");

                    b.Property<byte[]>("MagentoPayload")
                        .HasColumnType("longblob");

                    b.Property<int>("OrderId")
                        .HasColumnType("int");

                    b.Property<string>("RetriedBy")
                        .HasColumnType("longtext");

                    b.Property<int>("RetryCount")
                        .HasColumnType("int");

                    b.Property<string>("Status")
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.ToTable("Order");
                });

            modelBuilder.Entity("Kps.Integration.Domain.Entities.OrderItem", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("GetflyProductCode")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<int?>("GetflyProductId")
                        .HasColumnType("int");

                    b.Property<int>("OrderId")
                        .HasColumnType("int");

                    b.Property<int>("OrderItemId")
                        .HasColumnType("int");

                    b.Property<int>("ProductId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("OrderItem");
                });

            modelBuilder.Entity("Kps.Integration.Domain.Entities.ProductMapping", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("GetflyProductCode")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<int>("GetflyProductId")
                        .HasColumnType("int");

                    b.Property<int>("ProductId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("ProductMapping");
                });

            modelBuilder.Entity("Kps.Integration.Domain.Entities.ScheduleLogging", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("ApplicationName")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("datetime(6)");

                    b.Property<DateTime>("LastOrderTime")
                        .HasColumnType("datetime(6)");

                    b.HasKey("Id");

                    b.ToTable("ScheduleLogging");
                });

            modelBuilder.Entity("Kps.Integration.Domain.Entities.WmsSyncLog", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<int>("OrderId")
                        .HasColumnType("int");

                    b.Property<string>("Payload")
                        .HasColumnType("longtext");

                    b.Property<bool>("Synced")
                        .HasColumnType("tinyint(1)");

                    b.Property<DateTime>("Updated")
                        .HasColumnType("datetime(6)");

                    b.HasKey("Id");

                    b.ToTable("WmsSyncLog");
                });
#pragma warning restore 612, 618
        }
    }
}
