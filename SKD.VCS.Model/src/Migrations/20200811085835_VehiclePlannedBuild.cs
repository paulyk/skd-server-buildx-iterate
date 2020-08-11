using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SKD.VCS.Model.src.Migrations
{
    public partial class VehiclePlannedBuild : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PlannedBuildAt",
                table: "vehicle",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PlannedBuildAt",
                table: "vehicle");
        }
    }
}
