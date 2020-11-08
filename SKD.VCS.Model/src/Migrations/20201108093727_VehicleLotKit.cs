using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SKD.VCS.Model.src.Migrations
{
    public partial class VehicleLotKit : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_vehicle_LotNo",
                table: "vehicle");

            migrationBuilder.DropIndex(
                name: "IX_vehicle_VIN",
                table: "vehicle");

            migrationBuilder.DropColumn(
                name: "LotNo",
                table: "vehicle");

            migrationBuilder.DropColumn(
                name: "PlannedBuildAt",
                table: "vehicle");

            migrationBuilder.DropColumn(
                name: "ScanCompleteAt",
                table: "vehicle");

            migrationBuilder.AlterColumn<string>(
                name: "VIN",
                table: "vehicle",
                maxLength: 17,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(17)",
                oldMaxLength: 17);

            migrationBuilder.AlterColumn<string>(
                name: "KitNo",
                table: "vehicle",
                maxLength: 17,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(17)",
                oldMaxLength: 17,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_KitNo",
                table: "vehicle",
                column: "KitNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_VIN",
                table: "vehicle",
                column: "VIN");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_vehicle_KitNo",
                table: "vehicle");

            migrationBuilder.DropIndex(
                name: "IX_vehicle_VIN",
                table: "vehicle");

            migrationBuilder.AlterColumn<string>(
                name: "VIN",
                table: "vehicle",
                type: "nvarchar(17)",
                maxLength: 17,
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 17,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "KitNo",
                table: "vehicle",
                type: "nvarchar(17)",
                maxLength: 17,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 17);

            migrationBuilder.AddColumn<string>(
                name: "LotNo",
                table: "vehicle",
                type: "nvarchar(15)",
                maxLength: 15,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PlannedBuildAt",
                table: "vehicle",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ScanCompleteAt",
                table: "vehicle",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_LotNo",
                table: "vehicle",
                column: "LotNo");

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_VIN",
                table: "vehicle",
                column: "VIN",
                unique: true);
        }
    }
}
