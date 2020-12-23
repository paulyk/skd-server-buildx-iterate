using Microsoft.EntityFrameworkCore.Migrations;

namespace SKD.Model.src.Migrations
{
    public partial class PartDesc : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_part_PartDesc",
                table: "part");

            migrationBuilder.CreateIndex(
                name: "IX_part_PartDesc",
                table: "part",
                column: "PartDesc");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_part_PartDesc",
                table: "part");

            migrationBuilder.CreateIndex(
                name: "IX_part_PartDesc",
                table: "part",
                column: "PartDesc",
                unique: true,
                filter: "[PartDesc] IS NOT NULL");
        }
    }
}
