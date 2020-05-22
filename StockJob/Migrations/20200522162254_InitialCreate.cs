using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace StockJob.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StockHistory",
                columns: table => new
                {
                    StockHistoryID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    No = table.Column<string>(maxLength: 50, nullable: false),
                    Type = table.Column<string>(maxLength: 50, nullable: false),
                    Name = table.Column<string>(maxLength: 50, nullable: false),
                    Date = table.Column<DateTime>(nullable: false),
                    TradeVolume = table.Column<long>(nullable: false),
                    TurnOverInValue = table.Column<decimal>(type: "decimal(16,2)", nullable: false),
                    OpeningPrice = table.Column<decimal>(type: "decimal(16,2)", nullable: false),
                    HighestPrice = table.Column<decimal>(type: "decimal(16,2)", nullable: false),
                    LowestPrice = table.Column<decimal>(type: "decimal(16,2)", nullable: false),
                    ClosingPrice = table.Column<decimal>(type: "decimal(16,2)", nullable: false),
                    DailyPricing = table.Column<string>(nullable: true),
                    NumberOfDeals = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockHistory", x => x.StockHistoryID);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StockHistory");
        }
    }
}
