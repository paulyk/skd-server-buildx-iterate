using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace VT.Model.src.Migrations
{
    public partial class VehicleComponent_Completed : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedAt",
                table: "vehicle_component",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompletedAt",
                table: "vehicle_component");
        }
    }
}
