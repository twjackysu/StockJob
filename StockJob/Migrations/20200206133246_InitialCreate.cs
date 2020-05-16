using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace StockJob.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StockInfo",
                columns: table => new
                {
                    StockInfoID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    No = table.Column<string>(maxLength: 50, nullable: false),
                    Type = table.Column<string>(maxLength: 50, nullable: false),
                    Name = table.Column<string>(maxLength: 50, nullable: false),
                    FullName = table.Column<string>(maxLength: 100, nullable: false),
                    LastTradedPrice = table.Column<decimal>(type: "decimal(16,2)", nullable: false),
                    LastVolume = table.Column<int>(nullable: false),
                    TotalVolume = table.Column<int>(nullable: false),
                    SyncTime = table.Column<DateTime>(nullable: false),
                    HighestPrice = table.Column<decimal>(type: "decimal(16,2)", nullable: false),
                    LowestPrice = table.Column<decimal>(type: "decimal(16,2)", nullable: false),
                    OpeningPrice = table.Column<decimal>(type: "decimal(16,2)", nullable: false),
                    YesterdayClosingPrice = table.Column<decimal>(type: "decimal(16,2)", nullable: false),
                    LimitUp = table.Column<decimal>(type: "decimal(16,2)", nullable: false),
                    LimitDown = table.Column<decimal>(type: "decimal(16,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockInfo", x => x.StockInfoID);
                });

            migrationBuilder.CreateTable(
                name: "Top5Buy",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Price = table.Column<decimal>(type: "decimal(16,2)", nullable: false),
                    Volume = table.Column<int>(nullable: false),
                    StockInfoID = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Top5Buy", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Top5Buy_StockInfo_StockInfoID",
                        column: x => x.StockInfoID,
                        principalTable: "StockInfo",
                        principalColumn: "StockInfoID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Top5Sell",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Price = table.Column<decimal>(type: "decimal(16,2)", nullable: false),
                    Volume = table.Column<int>(nullable: false),
                    StockInfoID = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Top5Sell", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Top5Sell_StockInfo_StockInfoID",
                        column: x => x.StockInfoID,
                        principalTable: "StockInfo",
                        principalColumn: "StockInfoID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Top5Buy_StockInfoID",
                table: "Top5Buy",
                column: "StockInfoID");

            migrationBuilder.CreateIndex(
                name: "IX_Top5Sell_StockInfoID",
                table: "Top5Sell",
                column: "StockInfoID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Top5Buy");

            migrationBuilder.DropTable(
                name: "Top5Sell");

            migrationBuilder.DropTable(
                name: "StockInfo");
        }
    }
}
