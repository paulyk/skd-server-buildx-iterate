using Microsoft.EntityFrameworkCore.Migrations;

namespace SKD.Model.src.Migrations
{
    public partial class Component_FordComponentType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Type",
                table: "component",
                newName: "FordComponentType");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
             migrationBuilder.RenameColumn(
                name: "FordComponentType",
                table: "component",
                newName: "Type");
        }
    }
}
