using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SKD.Model.src.Migrations
{
    public partial class ComponentSerialRule : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_production_station_Name",
                table: "production_station");

            migrationBuilder.DropIndex(
                name: "IX_kit_timeline_event_KitId",
                table: "kit_timeline_event");

            migrationBuilder.DropColumn(
                name: "DcwsSerialCaptureRule",
                table: "component");

            migrationBuilder.AlterColumn<string>(
                name: "Series",
                table: "vehicle_model",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ModelYear",
                table: "vehicle_model",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Model",
                table: "vehicle_model",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Body",
                table: "vehicle_model",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "production_station",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PartDesc",
                table: "part",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ComponentSerialRule",
                table: "component",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "ONE_OR_BOTH_SERIALS");

            migrationBuilder.AddColumn<bool>(
                name: "DcwsRequired",
                table: "component",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.CreateIndex(
                name: "IX_production_station_Name",
                table: "production_station",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_kit_timeline_event_KitId_CreatedAt",
                table: "kit_timeline_event",
                columns: new[] { "KitId", "CreatedAt" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_production_station_Name",
                table: "production_station");

            migrationBuilder.DropIndex(
                name: "IX_kit_timeline_event_KitId_CreatedAt",
                table: "kit_timeline_event");

            migrationBuilder.DropColumn(
                name: "ComponentSerialRule",
                table: "component");

            migrationBuilder.DropColumn(
                name: "DcwsRequired",
                table: "component");

            migrationBuilder.AlterColumn<string>(
                name: "Series",
                table: "vehicle_model",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "ModelYear",
                table: "vehicle_model",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Model",
                table: "vehicle_model",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Body",
                table: "vehicle_model",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "production_station",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "PartDesc",
                table: "part",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<int>(
                name: "DcwsSerialCaptureRule",
                table: "component",
                type: "int",
                maxLength: 30,
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_production_station_Name",
                table: "production_station",
                column: "Name",
                unique: true,
                filter: "[Name] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_kit_timeline_event_KitId",
                table: "kit_timeline_event",
                column: "KitId");
        }
    }
}
