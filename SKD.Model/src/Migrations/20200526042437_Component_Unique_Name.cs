using Microsoft.EntityFrameworkCore.Migrations;

namespace SKD.Model.src.Migrations
{
    public partial class Component_Unique_Name : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_component_Name",
                table: "component");

            migrationBuilder.CreateIndex(
                name: "IX_component_Name",
                table: "component",
                column: "Name",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_component_Name",
                table: "component");

            migrationBuilder.CreateIndex(
                name: "IX_component_Name",
                table: "component",
                column: "Name");
        }
    }
}
