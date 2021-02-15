using Microsoft.EntityFrameworkCore.Migrations;

namespace SKD.Model.src.Migrations
{
    public partial class LotPart : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_lot_part_LotId_PartId",
                table: "lot_part");

            migrationBuilder.CreateIndex(
                name: "IX_lot_part_LotId_PartId",
                table: "lot_part",
                columns: new[] { "LotId", "PartId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_lot_part_LotId_PartId",
                table: "lot_part");

            migrationBuilder.CreateIndex(
                name: "IX_lot_part_LotId_PartId",
                table: "lot_part",
                columns: new[] { "LotId", "PartId" },
                unique: true);
        }
    }
}
