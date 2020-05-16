using Microsoft.EntityFrameworkCore.Migrations;

namespace VT.Model.Migrations
{
    public partial class VehicleKitLotNo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "KitNo",
                table: "vehicle",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LotNo",
                table: "vehicle",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "KitNo",
                table: "vehicle");

            migrationBuilder.DropColumn(
                name: "LotNo",
                table: "vehicle");
        }
    }
}
