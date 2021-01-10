using Microsoft.EntityFrameworkCore.Migrations;

namespace SKD.Model.src.Migrations
{
    public partial class VerifiedAt : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ScanVerifiedAt",
                table: "vehicle_component",
                newName: "VerifiedAt");

            migrationBuilder.RenameColumn(
                name: "AcceptedAt",
                table: "component_serial",
                newName: "VerifiedAt");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "VerifiedAt",
                table: "vehicle_component",
                newName: "ScanVerifiedAt");

            migrationBuilder.RenameColumn(
                name: "VerifiedAt",
                table: "component_serial",
                newName: "AcceptedAt");
        }
    }
}
