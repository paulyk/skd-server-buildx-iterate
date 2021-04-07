using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SKD.Model.src.Migrations
{
    public partial class KitVinImport : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "kit_vin_import",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    PlantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PartnerPlantCode = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    Sequence = table.Column<int>(type: "int", maxLength: 6, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RemovedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_kit_vin_import", x => x.Id);
                    table.ForeignKey(
                        name: "FK_kit_vin_import_plant_PlantId",
                        column: x => x.PlantId,
                        principalTable: "plant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "kit_vin",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    KitVinImportId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    KitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VIN = table.Column<string>(type: "nvarchar(17)", maxLength: 17, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RemovedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_kit_vin", x => x.Id);
                    table.ForeignKey(
                        name: "FK_kit_vin_kit_KitId",
                        column: x => x.KitId,
                        principalTable: "kit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_kit_vin_kit_vin_import_KitVinImportId",
                        column: x => x.KitVinImportId,
                        principalTable: "kit_vin_import",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_kit_vin_KitId",
                table: "kit_vin",
                column: "KitId");

            migrationBuilder.CreateIndex(
                name: "IX_kit_vin_KitVinImportId",
                table: "kit_vin",
                column: "KitVinImportId");

            migrationBuilder.CreateIndex(
                name: "IX_kit_vin_VIN",
                table: "kit_vin",
                column: "VIN",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_kit_vin_import_PlantId_Sequence",
                table: "kit_vin_import",
                columns: new[] { "PlantId", "Sequence" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "kit_vin");

            migrationBuilder.DropTable(
                name: "kit_vin_import");
        }
    }
}
