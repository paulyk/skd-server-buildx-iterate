using Microsoft.EntityFrameworkCore.Migrations;

namespace SKD.Model.src.Migrations
{
    public partial class VehicleModelMods : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_vehicle_model_Name",
                table: "vehicle_model");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "vehicle_model");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "vehicle_model",
                newName: "Description");

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "vehicle_model",
                type: "nvarchar(7)",
                maxLength: 7,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(11)",
                oldMaxLength: 11);

            migrationBuilder.AddColumn<string>(
                name: "Body",
                table: "vehicle_model",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Model",
                table: "vehicle_model",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModelYear",
                table: "vehicle_model",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Series",
                table: "vehicle_model",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Body",
                table: "vehicle_model");

            migrationBuilder.DropColumn(
                name: "Model",
                table: "vehicle_model");

            migrationBuilder.DropColumn(
                name: "ModelYear",
                table: "vehicle_model");

            migrationBuilder.DropColumn(
                name: "Series",
                table: "vehicle_model");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "vehicle_model",
                newName: "Name");

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "vehicle_model",
                type: "nvarchar(11)",
                maxLength: 11,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(7)",
                oldMaxLength: 7);

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "vehicle_model",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_model_Name",
                table: "vehicle_model",
                column: "Name",
                unique: true);
        }
    }
}
