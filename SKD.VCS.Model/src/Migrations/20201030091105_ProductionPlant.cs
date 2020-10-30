using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SKD.VCS.Model.src.Migrations
{
    public partial class ProductionPlant : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ProductionPlantId",
                table: "shipment",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "production_plant",
                columns: table => new
                {
                    Id = table.Column<Guid>(maxLength: 36, nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    RemovedAt = table.Column<DateTime>(nullable: true),
                    Code = table.Column<string>(maxLength: 10, nullable: false),
                    Name = table.Column<string>(maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_production_plant", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_shipment_ProductionPlantId",
                table: "shipment",
                column: "ProductionPlantId");

            migrationBuilder.CreateIndex(
                name: "IX_production_plant_Code",
                table: "production_plant",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_production_plant_Name",
                table: "production_plant",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_shipment_production_plant_ProductionPlantId",
                table: "shipment",
                column: "ProductionPlantId",
                principalTable: "production_plant",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_shipment_production_plant_ProductionPlantId",
                table: "shipment");

            migrationBuilder.DropTable(
                name: "production_plant");

            migrationBuilder.DropIndex(
                name: "IX_shipment_ProductionPlantId",
                table: "shipment");

            migrationBuilder.DropColumn(
                name: "ProductionPlantId",
                table: "shipment");
        }
    }
}
