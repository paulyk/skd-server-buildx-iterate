using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SKD.Model.src.Migrations
{
    public partial class RemoveKitModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_kit_vehicle_model_ModelId",
                table: "kit");

            migrationBuilder.DropIndex(
                name: "IX_kit_ModelId",
                table: "kit");

            migrationBuilder.DropColumn(
                name: "ModelId",
                table: "kit");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ModelId",
                table: "kit",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_kit_ModelId",
                table: "kit",
                column: "ModelId");

            migrationBuilder.AddForeignKey(
                name: "FK_kit_vehicle_model_ModelId",
                table: "kit",
                column: "ModelId",
                principalTable: "vehicle_model",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
