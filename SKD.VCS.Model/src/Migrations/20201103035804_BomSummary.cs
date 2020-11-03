using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SKD.VCS.Model.src.Migrations
{
    public partial class BomSummary : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "bom_summary",
                columns: table => new
                {
                    Id = table.Column<Guid>(maxLength: 36, nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    RemovedAt = table.Column<DateTime>(nullable: true),
                    SequenceNo = table.Column<string>(maxLength: 4, nullable: false),
                    LotPartQuantitiesMatchShipment = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bom_summary", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "bom_summary_part",
                columns: table => new
                {
                    Id = table.Column<Guid>(maxLength: 36, nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    RemovedAt = table.Column<DateTime>(nullable: true),
                    LotNo = table.Column<string>(maxLength: 15, nullable: false),
                    PartNo = table.Column<string>(maxLength: 30, nullable: false),
                    PartDesc = table.Column<string>(maxLength: 34, nullable: false),
                    Quantity = table.Column<int>(nullable: false),
                    MatcheShipmentLotPartQuantity = table.Column<bool>(nullable: false),
                    BomSummaryId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bom_summary_part", x => x.Id);
                    table.ForeignKey(
                        name: "FK_bom_summary_part_bom_summary_BomSummaryId",
                        column: x => x.BomSummaryId,
                        principalTable: "bom_summary",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_bom_summary_SequenceNo",
                table: "bom_summary",
                column: "SequenceNo");

            migrationBuilder.CreateIndex(
                name: "IX_bom_summary_part_BomSummaryId",
                table: "bom_summary_part",
                column: "BomSummaryId");

            migrationBuilder.CreateIndex(
                name: "IX_bom_summary_part_PartNo",
                table: "bom_summary_part",
                column: "PartNo");

            migrationBuilder.CreateIndex(
                name: "IX_bom_summary_part_LotNo_PartNo",
                table: "bom_summary_part",
                columns: new[] { "LotNo", "PartNo" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bom_summary_part");

            migrationBuilder.DropTable(
                name: "bom_summary");
        }
    }
}
