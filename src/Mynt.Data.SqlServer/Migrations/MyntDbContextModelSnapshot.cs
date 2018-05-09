﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Mynt.Core.Enums;
using Mynt.Data.SqlServer;
using System;

namespace Mynt.Data.SqlServer.Migrations
{
    [DbContext(typeof(MyntDbContext))]
    partial class MyntDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.0.2-rtm-10011")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Mynt.Data.SqlServer.TradeAdapter", b =>
                {
                    b.Property<int>("TradeId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("BuyOrderId");

                    b.Property<DateTime?>("CloseDate");

                    b.Property<double?>("CloseProfit");

                    b.Property<double?>("CloseProfitPercentage");

                    b.Property<double?>("CloseRate");

                    b.Property<bool>("IsBuying");

                    b.Property<bool>("IsOpen");

                    b.Property<bool>("IsSelling");

                    b.Property<string>("Market");

                    b.Property<DateTime>("OpenDate");

                    b.Property<string>("OpenOrderId");

                    b.Property<double>("OpenRate");

                    b.Property<double>("Quantity");

                    b.Property<string>("SellOrderId");

                    b.Property<int>("SellType");

                    b.Property<double>("StakeAmount");

                    b.Property<double?>("StopLossRate");

                    b.Property<string>("StrategyUsed");

                    b.Property<string>("TraderId");

                    b.HasKey("TradeId");

                    b.ToTable("Orders");
                });

            modelBuilder.Entity("Mynt.Data.SqlServer.TraderAdapter", b =>
                {
                    b.Property<int>("TraderId")
                        .ValueGeneratedOnAdd();

                    b.Property<double>("CurrentBalance");

                    b.Property<string>("Identifier");

                    b.Property<bool>("IsBusy");

                    b.Property<bool>("IsArchived");

                    b.Property<DateTime>("LastUpdated");

                    b.Property<double>("StakeAmount");

                    b.HasKey("TraderId");

                    b.ToTable("Traders");
                });
#pragma warning restore 612, 618
        }
    }
}
