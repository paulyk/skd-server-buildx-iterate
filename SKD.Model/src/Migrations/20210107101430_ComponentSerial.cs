using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SKD.Model.src.Migrations
{
    public partial class ComponentSerial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_dcws_response_component_scan_ComponentScanId",
                table: "dcws_response");

            migrationBuilder.DropTable(
                name: "component_scan");

            migrationBuilder.CreateTable(
                name: "component_serial",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    VehicleComponentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Serial1 = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Serial2 = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AcceptedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RemovedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_component_serial", x => x.Id);
                    table.ForeignKey(
                        name: "FK_component_serial_vehicle_component_VehicleComponentId",
                        column: x => x.VehicleComponentId,
                        principalTable: "vehicle_component",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_component_serial_Serial1",
                table: "component_serial",
                column: "Serial1");

            migrationBuilder.CreateIndex(
                name: "IX_component_serial_Serial2",
                table: "component_serial",
                column: "Serial2");

            migrationBuilder.CreateIndex(
                name: "IX_component_serial_VehicleComponentId",
                table: "component_serial",
                column: "VehicleComponentId");

            migrationBuilder.AddForeignKey(
                name: "FK_dcws_response_component_serial_ComponentScanId",
                table: "dcws_response",
                column: "ComponentScanId",
                principalTable: "component_serial",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_dcws_response_component_serial_ComponentScanId",
                table: "dcws_response");

            migrationBuilder.DropTable(
                name: "component_serial");

            migrationBuilder.CreateTable(
                name: "component_scan",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    AcceptedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RemovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Scan1 = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Scan2 = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    VehicleComponentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_component_scan", x => x.Id);
                    table.ForeignKey(
                        name: "FK_component_scan_vehicle_component_VehicleComponentId",
                        column: x => x.VehicleComponentId,
                        principalTable: "vehicle_component",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_component_scan_Scan1",
                table: "component_scan",
                column: "Scan1");

            migrationBuilder.CreateIndex(
                name: "IX_component_scan_Scan2",
                table: "component_scan",
                column: "Scan2");

            migrationBuilder.CreateIndex(
                name: "IX_component_scan_VehicleComponentId",
                table: "component_scan",
                column: "VehicleComponentId");

            migrationBuilder.AddForeignKey(
                name: "FK_dcws_response_component_scan_ComponentScanId",
                table: "dcws_response",
                column: "ComponentScanId",
                principalTable: "component_scan",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
