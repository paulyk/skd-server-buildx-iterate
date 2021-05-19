using Microsoft.EntityFrameworkCore.Migrations;

namespace SKD.Model.src.Migrations
{
    public partial class OriginalSerial2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Original_Serial2",
                table: "component_serial",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Original_Serial2",
                table: "component_serial");
        }
    }
}
