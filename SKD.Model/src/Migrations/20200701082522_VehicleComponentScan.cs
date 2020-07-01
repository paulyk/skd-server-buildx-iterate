using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SKD.Model.src.Migrations
{
    public partial class VehicleComponentScan : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_vehicle_component_Scan1",
                table: "vehicle_component");

            migrationBuilder.DropIndex(
                name: "IX_vehicle_component_Scan2",
                table: "vehicle_component");

            migrationBuilder.DropColumn(
                name: "Scan1",
                table: "vehicle_component");

            migrationBuilder.DropColumn(
                name: "Scan2",
                table: "vehicle_component");

            migrationBuilder.DropColumn(
                name: "ScanAt",
                table: "vehicle_component");

            migrationBuilder.CreateTable(
                name: "VehicleComponentScans",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    RemovedAt = table.Column<DateTime>(nullable: true),
                    VehicleComponentId = table.Column<Guid>(nullable: false),
                    Scan1 = table.Column<string>(nullable: true),
                    Scan2 = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleComponentScans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VehicleComponentScans_vehicle_component_VehicleComponentId",
                        column: x => x.VehicleComponentId,
                        principalTable: "vehicle_component",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VehicleComponentScans_VehicleComponentId",
                table: "VehicleComponentScans",
                column: "VehicleComponentId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VehicleComponentScans");

            migrationBuilder.AddColumn<string>(
                name: "Scan1",
                table: "vehicle_component",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Scan2",
                table: "vehicle_component",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ScanAt",
                table: "vehicle_component",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_component_Scan1",
                table: "vehicle_component",
                column: "Scan1");

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_component_Scan2",
                table: "vehicle_component",
                column: "Scan2");
        }
    }
}
