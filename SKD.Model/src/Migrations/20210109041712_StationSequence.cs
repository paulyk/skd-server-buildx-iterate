using Microsoft.EntityFrameworkCore.Migrations;

namespace SKD.Model.src.Migrations
{
    public partial class StationSequence : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_component_serial_Serial1",
                table: "component_serial");

            migrationBuilder.RenameColumn(
                name: "SortOrder",
                table: "production_station",
                newName: "Sequence");

            migrationBuilder.CreateIndex(
                name: "IX_component_serial_Serial1_Serial2",
                table: "component_serial",
                columns: new[] { "Serial1", "Serial2" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_component_serial_Serial1_Serial2",
                table: "component_serial");

            migrationBuilder.RenameColumn(
                name: "Sequence",
                table: "production_station",
                newName: "SortOrder");

            migrationBuilder.CreateIndex(
                name: "IX_component_serial_Serial1",
                table: "component_serial",
                column: "Serial1");
        }
    }
}
