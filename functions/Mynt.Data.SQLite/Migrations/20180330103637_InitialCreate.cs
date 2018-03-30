using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Mynt.Data.Sqlite.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    TradeId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BuyOrderId = table.Column<string>(nullable: true),
                    CloseDate = table.Column<DateTime>(nullable: true),
                    CloseProfit = table.Column<double>(nullable: true),
                    CloseProfitPercentage = table.Column<double>(nullable: true),
                    CloseRate = table.Column<double>(nullable: true),
                    IsBuying = table.Column<bool>(nullable: false),
                    IsOpen = table.Column<bool>(nullable: false),
                    IsSelling = table.Column<bool>(nullable: false),
                    Market = table.Column<string>(nullable: true),
                    OpenDate = table.Column<DateTime>(nullable: false),
                    OpenOrderId = table.Column<string>(nullable: true),
                    OpenRate = table.Column<double>(nullable: false),
                    Quantity = table.Column<double>(nullable: false),
                    SellOrderId = table.Column<string>(nullable: true),
                    SellType = table.Column<int>(nullable: false),
                    StakeAmount = table.Column<double>(nullable: false),
                    StopLossRate = table.Column<double>(nullable: true),
                    StrategyUsed = table.Column<string>(nullable: true),
                    TraderId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.TradeId);
                });

            migrationBuilder.CreateTable(
                name: "Traders",
                columns: table => new
                {
                    TraderId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CurrentBalance = table.Column<double>(nullable: false),
                    Identifier = table.Column<string>(nullable: true),
                    IsBusy = table.Column<bool>(nullable: false),
                    LastUpdated = table.Column<DateTime>(nullable: false),
                    StakeAmount = table.Column<double>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Traders", x => x.TraderId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "Traders");
        }
    }
}
