using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SKD.Model.src.Migrations
{
    public partial class PCV : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_lot_vehicle_model_ModelId",
                table: "lot");

            migrationBuilder.DropForeignKey(
                name: "FK_vehicle_model_component_component_ComponentId",
                table: "vehicle_model_component");

            migrationBuilder.DropForeignKey(
                name: "FK_vehicle_model_component_production_station_ProductionStationId",
                table: "vehicle_model_component");

            migrationBuilder.DropForeignKey(
                name: "FK_vehicle_model_component_vehicle_model_VehicleModelId",
                table: "vehicle_model_component");

            migrationBuilder.DropPrimaryKey(
                name: "PK_vehicle_model_component",
                table: "vehicle_model_component");

            migrationBuilder.DropPrimaryKey(
                name: "PK_vehicle_model",
                table: "vehicle_model");

            migrationBuilder.RenameTable(
                name: "vehicle_model_component",
                newName: "pcv_component");

            migrationBuilder.RenameTable(
                name: "vehicle_model",
                newName: "pcv");

            migrationBuilder.RenameColumn(
                name: "ModelId",
                table: "lot",
                newName: "PcvId");

            migrationBuilder.RenameIndex(
                name: "IX_lot_ModelId",
                table: "lot",
                newName: "IX_lot_PcvId");

            migrationBuilder.RenameColumn(
                name: "VehicleModelId",
                table: "pcv_component",
                newName: "PcvId");

            migrationBuilder.RenameIndex(
                name: "IX_vehicle_model_component_VehicleModelId_ComponentId_ProductionStationId",
                table: "pcv_component",
                newName: "IX_pcv_component_PcvId_ComponentId_ProductionStationId");

            migrationBuilder.RenameIndex(
                name: "IX_vehicle_model_component_ProductionStationId",
                table: "pcv_component",
                newName: "IX_pcv_component_ProductionStationId");

            migrationBuilder.RenameIndex(
                name: "IX_vehicle_model_component_ComponentId",
                table: "pcv_component",
                newName: "IX_pcv_component_ComponentId");

            migrationBuilder.RenameIndex(
                name: "IX_vehicle_model_Code",
                table: "pcv",
                newName: "IX_pcv_Code");

            migrationBuilder.AddPrimaryKey(
                name: "PK_pcv_component",
                table: "pcv_component",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_pcv",
                table: "pcv",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_lot_pcv_PcvId",
                table: "lot",
                column: "PcvId",
                principalTable: "pcv",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_pcv_component_component_ComponentId",
                table: "pcv_component",
                column: "ComponentId",
                principalTable: "component",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_pcv_component_pcv_PcvId",
                table: "pcv_component",
                column: "PcvId",
                principalTable: "pcv",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_pcv_component_production_station_ProductionStationId",
                table: "pcv_component",
                column: "ProductionStationId",
                principalTable: "production_station",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_lot_pcv_PcvId",
                table: "lot");

            migrationBuilder.DropForeignKey(
                name: "FK_pcv_component_component_ComponentId",
                table: "pcv_component");

            migrationBuilder.DropForeignKey(
                name: "FK_pcv_component_pcv_PcvId",
                table: "pcv_component");

            migrationBuilder.DropForeignKey(
                name: "FK_pcv_component_production_station_ProductionStationId",
                table: "pcv_component");

            migrationBuilder.DropPrimaryKey(
                name: "PK_pcv_component",
                table: "pcv_component");

            migrationBuilder.DropPrimaryKey(
                name: "PK_pcv",
                table: "pcv");

            migrationBuilder.RenameTable(
                name: "pcv_component",
                newName: "vehicle_model_component");

            migrationBuilder.RenameTable(
                name: "pcv",
                newName: "vehicle_model");

            migrationBuilder.RenameColumn(
                name: "PcvId",
                table: "lot",
                newName: "ModelId");

            migrationBuilder.RenameIndex(
                name: "IX_lot_PcvId",
                table: "lot",
                newName: "IX_lot_ModelId");

            migrationBuilder.RenameColumn(
                name: "PcvId",
                table: "vehicle_model_component",
                newName: "VehicleModelId");

            migrationBuilder.RenameIndex(
                name: "IX_pcv_component_ProductionStationId",
                table: "vehicle_model_component",
                newName: "IX_vehicle_model_component_ProductionStationId");

            migrationBuilder.RenameIndex(
                name: "IX_pcv_component_PcvId_ComponentId_ProductionStationId",
                table: "vehicle_model_component",
                newName: "IX_vehicle_model_component_VehicleModelId_ComponentId_ProductionStationId");

            migrationBuilder.RenameIndex(
                name: "IX_pcv_component_ComponentId",
                table: "vehicle_model_component",
                newName: "IX_vehicle_model_component_ComponentId");

            migrationBuilder.RenameIndex(
                name: "IX_pcv_Code",
                table: "vehicle_model",
                newName: "IX_vehicle_model_Code");

            migrationBuilder.AddPrimaryKey(
                name: "PK_vehicle_model_component",
                table: "vehicle_model_component",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_vehicle_model",
                table: "vehicle_model",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_lot_vehicle_model_ModelId",
                table: "lot",
                column: "ModelId",
                principalTable: "vehicle_model",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_vehicle_model_component_component_ComponentId",
                table: "vehicle_model_component",
                column: "ComponentId",
                principalTable: "component",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_vehicle_model_component_production_station_ProductionStationId",
                table: "vehicle_model_component",
                column: "ProductionStationId",
                principalTable: "production_station",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_vehicle_model_component_vehicle_model_VehicleModelId",
                table: "vehicle_model_component",
                column: "VehicleModelId",
                principalTable: "vehicle_model",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
