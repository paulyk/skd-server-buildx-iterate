using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SKD.Model.src.Migrations
{
    public partial class ModelSubmodel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SubModelId",
                table: "pcv",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "IconURL",
                table: "component",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProductionStationId",
                table: "component",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "pcv_model",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RemovedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pcv_model", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "pcv_submodel",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ModelId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RemovedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pcv_submodel", x => x.Id);
                    table.ForeignKey(
                        name: "FK_pcv_submodel_pcv_model_ModelId",
                        column: x => x.ModelId,
                        principalTable: "pcv_model",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "pcv_submodel_component",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    SubmodelId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ComponentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RemovedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pcv_submodel_component", x => x.Id);
                    table.ForeignKey(
                        name: "FK_pcv_submodel_component_component_ComponentId",
                        column: x => x.ComponentId,
                        principalTable: "component",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_pcv_submodel_component_pcv_submodel_SubmodelId",
                        column: x => x.SubmodelId,
                        principalTable: "pcv_submodel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_pcv_SubModelId",
                table: "pcv",
                column: "SubModelId");

            migrationBuilder.CreateIndex(
                name: "IX_component_ProductionStationId",
                table: "component",
                column: "ProductionStationId");

            migrationBuilder.CreateIndex(
                name: "IX_pcv_model_Code",
                table: "pcv_model",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_pcv_model_Name",
                table: "pcv_model",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_pcv_submodel_Code",
                table: "pcv_submodel",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_pcv_submodel_ModelId",
                table: "pcv_submodel",
                column: "ModelId");

            migrationBuilder.CreateIndex(
                name: "IX_pcv_submodel_Name",
                table: "pcv_submodel",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_pcv_submodel_component_ComponentId",
                table: "pcv_submodel_component",
                column: "ComponentId");

            migrationBuilder.CreateIndex(
                name: "IX_pcv_submodel_component_SubmodelId",
                table: "pcv_submodel_component",
                column: "SubmodelId");

            migrationBuilder.AddForeignKey(
                name: "FK_component_production_station_ProductionStationId",
                table: "component",
                column: "ProductionStationId",
                principalTable: "production_station",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_pcv_pcv_submodel_SubModelId",
                table: "pcv",
                column: "SubModelId",
                principalTable: "pcv_submodel",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_component_production_station_ProductionStationId",
                table: "component");

            migrationBuilder.DropForeignKey(
                name: "FK_pcv_pcv_submodel_SubModelId",
                table: "pcv");

            migrationBuilder.DropTable(
                name: "pcv_submodel_component");

            migrationBuilder.DropTable(
                name: "pcv_submodel");

            migrationBuilder.DropTable(
                name: "pcv_model");

            migrationBuilder.DropIndex(
                name: "IX_pcv_SubModelId",
                table: "pcv");

            migrationBuilder.DropIndex(
                name: "IX_component_ProductionStationId",
                table: "component");

            migrationBuilder.DropColumn(
                name: "SubModelId",
                table: "pcv");

            migrationBuilder.DropColumn(
                name: "ProductionStationId",
                table: "component");

            migrationBuilder.AlterColumn<string>(
                name: "IconURL",
                table: "component",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }
    }
}
