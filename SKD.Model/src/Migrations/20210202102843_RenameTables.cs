using Microsoft.EntityFrameworkCore.Migrations;

namespace SKD.Model.src.Migrations
{
    public partial class RenameTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_component_serial_vehicle_component_VehicleComponentId",
                table: "component_serial");

            migrationBuilder.DropForeignKey(
                name: "FK_lot_part_vehicle_lot_LotId",
                table: "lot_part");

            migrationBuilder.DropForeignKey(
                name: "FK_vehicle_vehicle_lot_LotId",
                table: "vehicle");

            migrationBuilder.DropForeignKey(
                name: "FK_vehicle_vehicle_model_ModelId",
                table: "vehicle");

            migrationBuilder.DropForeignKey(
                name: "FK_vehicle_component_component_ComponentId",
                table: "vehicle_component");

            migrationBuilder.DropForeignKey(
                name: "FK_vehicle_component_production_station_ProductionStationId",
                table: "vehicle_component");

            migrationBuilder.DropForeignKey(
                name: "FK_vehicle_component_vehicle_KitId",
                table: "vehicle_component");

            migrationBuilder.DropForeignKey(
                name: "FK_vehicle_lot_bom_BomId",
                table: "vehicle_lot");

            migrationBuilder.DropForeignKey(
                name: "FK_vehicle_lot_plant_PlantId",
                table: "vehicle_lot");

            migrationBuilder.DropForeignKey(
                name: "FK_vehicle_snapshot_vehicle_KitId",
                table: "vehicle_snapshot");

            migrationBuilder.DropForeignKey(
                name: "FK_vehicle_snapshot_vehicle_snapshot_run_VehicleSnapshotRunId",
                table: "vehicle_snapshot");

            migrationBuilder.DropForeignKey(
                name: "FK_vehicle_snapshot_run_plant_PlantId",
                table: "vehicle_snapshot_run");

            migrationBuilder.DropForeignKey(
                name: "FK_vehicle_timeline_event_vehicle_KitId",
                table: "vehicle_timeline_event");

            migrationBuilder.DropForeignKey(
                name: "FK_vehicle_timeline_event_vehicle_timeline_event_type_KitTimelineEventTypeId",
                table: "vehicle_timeline_event");

            migrationBuilder.DropPrimaryKey(
                name: "PK_vehicle_timeline_event_type",
                table: "vehicle_timeline_event_type");

            migrationBuilder.DropPrimaryKey(
                name: "PK_vehicle_timeline_event",
                table: "vehicle_timeline_event");

            migrationBuilder.DropPrimaryKey(
                name: "PK_vehicle_snapshot_run",
                table: "vehicle_snapshot_run");

            migrationBuilder.DropPrimaryKey(
                name: "PK_vehicle_snapshot",
                table: "vehicle_snapshot");

            migrationBuilder.DropPrimaryKey(
                name: "PK_vehicle_lot",
                table: "vehicle_lot");

            migrationBuilder.DropPrimaryKey(
                name: "PK_vehicle_component",
                table: "vehicle_component");

            migrationBuilder.DropPrimaryKey(
                name: "PK_vehicle",
                table: "vehicle");

            migrationBuilder.RenameTable(
                name: "vehicle_timeline_event_type",
                newName: "kit_timeline_event_type");

            migrationBuilder.RenameTable(
                name: "vehicle_timeline_event",
                newName: "kit_timeline_event");

            migrationBuilder.RenameTable(
                name: "vehicle_snapshot_run",
                newName: "kit_snapshot_run");

            migrationBuilder.RenameTable(
                name: "vehicle_snapshot",
                newName: "kit_snapshot");

            migrationBuilder.RenameTable(
                name: "vehicle_lot",
                newName: "lot");

            migrationBuilder.RenameTable(
                name: "vehicle_component",
                newName: "kit_component");

            migrationBuilder.RenameTable(
                name: "vehicle",
                newName: "kit");

            migrationBuilder.RenameColumn(
                name: "VehicleComponentId",
                table: "component_serial",
                newName: "KitComponentId");

            migrationBuilder.RenameIndex(
                name: "IX_component_serial_VehicleComponentId",
                table: "component_serial",
                newName: "IX_component_serial_KitComponentId");

            migrationBuilder.RenameIndex(
                name: "IX_vehicle_timeline_event_type_Code",
                table: "kit_timeline_event_type",
                newName: "IX_kit_timeline_event_type_Code");

            migrationBuilder.RenameIndex(
                name: "IX_vehicle_timeline_event_KitTimelineEventTypeId",
                table: "kit_timeline_event",
                newName: "IX_kit_timeline_event_KitTimelineEventTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_vehicle_timeline_event_KitId",
                table: "kit_timeline_event",
                newName: "IX_kit_timeline_event_KitId");

            migrationBuilder.RenameIndex(
                name: "IX_vehicle_timeline_event_CreatedAt",
                table: "kit_timeline_event",
                newName: "IX_kit_timeline_event_CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_vehicle_snapshot_run_PlantId_Sequence",
                table: "kit_snapshot_run",
                newName: "IX_kit_snapshot_run_PlantId_Sequence");

            migrationBuilder.RenameIndex(
                name: "IX_vehicle_snapshot_run_PlantId_RunDate",
                table: "kit_snapshot_run",
                newName: "IX_kit_snapshot_run_PlantId_RunDate");

            migrationBuilder.RenameColumn(
                name: "VehicleSnapshotRunId",
                table: "kit_snapshot",
                newName: "KitSnapshotRunId");

            migrationBuilder.RenameIndex(
                name: "IX_vehicle_snapshot_VehicleSnapshotRunId_KitId",
                table: "kit_snapshot",
                newName: "IX_kit_snapshot_KitSnapshotRunId_KitId");

            migrationBuilder.RenameIndex(
                name: "IX_vehicle_snapshot_KitId",
                table: "kit_snapshot",
                newName: "IX_kit_snapshot_KitId");

            migrationBuilder.RenameIndex(
                name: "IX_vehicle_lot_PlantId",
                table: "lot",
                newName: "IX_lot_PlantId");

            migrationBuilder.RenameIndex(
                name: "IX_vehicle_lot_LotNo",
                table: "lot",
                newName: "IX_lot_LotNo");

            migrationBuilder.RenameIndex(
                name: "IX_vehicle_lot_BomId",
                table: "lot",
                newName: "IX_lot_BomId");

            migrationBuilder.RenameIndex(
                name: "IX_vehicle_component_ProductionStationId",
                table: "kit_component",
                newName: "IX_kit_component_ProductionStationId");

            migrationBuilder.RenameIndex(
                name: "IX_vehicle_component_KitId_ComponentId_ProductionStationId",
                table: "kit_component",
                newName: "IX_kit_component_KitId_ComponentId_ProductionStationId");

            migrationBuilder.RenameIndex(
                name: "IX_vehicle_component_ComponentId",
                table: "kit_component",
                newName: "IX_kit_component_ComponentId");

            migrationBuilder.RenameIndex(
                name: "IX_vehicle_VIN",
                table: "kit",
                newName: "IX_kit_VIN");

            migrationBuilder.RenameIndex(
                name: "IX_vehicle_ModelId",
                table: "kit",
                newName: "IX_kit_ModelId");

            migrationBuilder.RenameIndex(
                name: "IX_vehicle_LotId",
                table: "kit",
                newName: "IX_kit_LotId");

            migrationBuilder.RenameIndex(
                name: "IX_vehicle_KitNo",
                table: "kit",
                newName: "IX_kit_KitNo");

            migrationBuilder.AddPrimaryKey(
                name: "PK_kit_timeline_event_type",
                table: "kit_timeline_event_type",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_kit_timeline_event",
                table: "kit_timeline_event",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_kit_snapshot_run",
                table: "kit_snapshot_run",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_kit_snapshot",
                table: "kit_snapshot",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_lot",
                table: "lot",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_kit_component",
                table: "kit_component",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_kit",
                table: "kit",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_component_serial_kit_component_KitComponentId",
                table: "component_serial",
                column: "KitComponentId",
                principalTable: "kit_component",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_kit_lot_LotId",
                table: "kit",
                column: "LotId",
                principalTable: "lot",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_kit_vehicle_model_ModelId",
                table: "kit",
                column: "ModelId",
                principalTable: "vehicle_model",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_kit_component_component_ComponentId",
                table: "kit_component",
                column: "ComponentId",
                principalTable: "component",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_kit_component_kit_KitId",
                table: "kit_component",
                column: "KitId",
                principalTable: "kit",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_kit_component_production_station_ProductionStationId",
                table: "kit_component",
                column: "ProductionStationId",
                principalTable: "production_station",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_kit_snapshot_kit_KitId",
                table: "kit_snapshot",
                column: "KitId",
                principalTable: "kit",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_kit_snapshot_kit_snapshot_run_KitSnapshotRunId",
                table: "kit_snapshot",
                column: "KitSnapshotRunId",
                principalTable: "kit_snapshot_run",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_kit_snapshot_run_plant_PlantId",
                table: "kit_snapshot_run",
                column: "PlantId",
                principalTable: "plant",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_kit_timeline_event_kit_KitId",
                table: "kit_timeline_event",
                column: "KitId",
                principalTable: "kit",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_kit_timeline_event_kit_timeline_event_type_KitTimelineEventTypeId",
                table: "kit_timeline_event",
                column: "KitTimelineEventTypeId",
                principalTable: "kit_timeline_event_type",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_lot_bom_BomId",
                table: "lot",
                column: "BomId",
                principalTable: "bom",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_lot_plant_PlantId",
                table: "lot",
                column: "PlantId",
                principalTable: "plant",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_lot_part_lot_LotId",
                table: "lot_part",
                column: "LotId",
                principalTable: "lot",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_component_serial_kit_component_KitComponentId",
                table: "component_serial");

            migrationBuilder.DropForeignKey(
                name: "FK_kit_lot_LotId",
                table: "kit");

            migrationBuilder.DropForeignKey(
                name: "FK_kit_vehicle_model_ModelId",
                table: "kit");

            migrationBuilder.DropForeignKey(
                name: "FK_kit_component_component_ComponentId",
                table: "kit_component");

            migrationBuilder.DropForeignKey(
                name: "FK_kit_component_kit_KitId",
                table: "kit_component");

            migrationBuilder.DropForeignKey(
                name: "FK_kit_component_production_station_ProductionStationId",
                table: "kit_component");

            migrationBuilder.DropForeignKey(
                name: "FK_kit_snapshot_kit_KitId",
                table: "kit_snapshot");

            migrationBuilder.DropForeignKey(
                name: "FK_kit_snapshot_kit_snapshot_run_KitSnapshotRunId",
                table: "kit_snapshot");

            migrationBuilder.DropForeignKey(
                name: "FK_kit_snapshot_run_plant_PlantId",
                table: "kit_snapshot_run");

            migrationBuilder.DropForeignKey(
                name: "FK_kit_timeline_event_kit_KitId",
                table: "kit_timeline_event");

            migrationBuilder.DropForeignKey(
                name: "FK_kit_timeline_event_kit_timeline_event_type_KitTimelineEventTypeId",
                table: "kit_timeline_event");

            migrationBuilder.DropForeignKey(
                name: "FK_lot_bom_BomId",
                table: "lot");

            migrationBuilder.DropForeignKey(
                name: "FK_lot_plant_PlantId",
                table: "lot");

            migrationBuilder.DropForeignKey(
                name: "FK_lot_part_lot_LotId",
                table: "lot_part");

            migrationBuilder.DropPrimaryKey(
                name: "PK_lot",
                table: "lot");

            migrationBuilder.DropPrimaryKey(
                name: "PK_kit_timeline_event_type",
                table: "kit_timeline_event_type");

            migrationBuilder.DropPrimaryKey(
                name: "PK_kit_timeline_event",
                table: "kit_timeline_event");

            migrationBuilder.DropPrimaryKey(
                name: "PK_kit_snapshot_run",
                table: "kit_snapshot_run");

            migrationBuilder.DropPrimaryKey(
                name: "PK_kit_snapshot",
                table: "kit_snapshot");

            migrationBuilder.DropPrimaryKey(
                name: "PK_kit_component",
                table: "kit_component");

            migrationBuilder.DropPrimaryKey(
                name: "PK_kit",
                table: "kit");

            migrationBuilder.RenameTable(
                name: "lot",
                newName: "vehicle_lot");

            migrationBuilder.RenameTable(
                name: "kit_timeline_event_type",
                newName: "vehicle_timeline_event_type");

            migrationBuilder.RenameTable(
                name: "kit_timeline_event",
                newName: "vehicle_timeline_event");

            migrationBuilder.RenameTable(
                name: "kit_snapshot_run",
                newName: "vehicle_snapshot_run");

            migrationBuilder.RenameTable(
                name: "kit_snapshot",
                newName: "vehicle_snapshot");

            migrationBuilder.RenameTable(
                name: "kit_component",
                newName: "vehicle_component");

            migrationBuilder.RenameTable(
                name: "kit",
                newName: "vehicle");

            migrationBuilder.RenameColumn(
                name: "KitComponentId",
                table: "component_serial",
                newName: "VehicleComponentId");

            migrationBuilder.RenameIndex(
                name: "IX_component_serial_KitComponentId",
                table: "component_serial",
                newName: "IX_component_serial_VehicleComponentId");

            migrationBuilder.RenameIndex(
                name: "IX_lot_PlantId",
                table: "vehicle_lot",
                newName: "IX_vehicle_lot_PlantId");

            migrationBuilder.RenameIndex(
                name: "IX_lot_LotNo",
                table: "vehicle_lot",
                newName: "IX_vehicle_lot_LotNo");

            migrationBuilder.RenameIndex(
                name: "IX_lot_BomId",
                table: "vehicle_lot",
                newName: "IX_vehicle_lot_BomId");

            migrationBuilder.RenameIndex(
                name: "IX_kit_timeline_event_type_Code",
                table: "vehicle_timeline_event_type",
                newName: "IX_vehicle_timeline_event_type_Code");

            migrationBuilder.RenameIndex(
                name: "IX_kit_timeline_event_KitTimelineEventTypeId",
                table: "vehicle_timeline_event",
                newName: "IX_vehicle_timeline_event_KitTimelineEventTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_kit_timeline_event_KitId",
                table: "vehicle_timeline_event",
                newName: "IX_vehicle_timeline_event_KitId");

            migrationBuilder.RenameIndex(
                name: "IX_kit_timeline_event_CreatedAt",
                table: "vehicle_timeline_event",
                newName: "IX_vehicle_timeline_event_CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_kit_snapshot_run_PlantId_Sequence",
                table: "vehicle_snapshot_run",
                newName: "IX_vehicle_snapshot_run_PlantId_Sequence");

            migrationBuilder.RenameIndex(
                name: "IX_kit_snapshot_run_PlantId_RunDate",
                table: "vehicle_snapshot_run",
                newName: "IX_vehicle_snapshot_run_PlantId_RunDate");

            migrationBuilder.RenameColumn(
                name: "KitSnapshotRunId",
                table: "vehicle_snapshot",
                newName: "VehicleSnapshotRunId");

            migrationBuilder.RenameIndex(
                name: "IX_kit_snapshot_KitSnapshotRunId_KitId",
                table: "vehicle_snapshot",
                newName: "IX_vehicle_snapshot_VehicleSnapshotRunId_KitId");

            migrationBuilder.RenameIndex(
                name: "IX_kit_snapshot_KitId",
                table: "vehicle_snapshot",
                newName: "IX_vehicle_snapshot_KitId");

            migrationBuilder.RenameIndex(
                name: "IX_kit_component_ProductionStationId",
                table: "vehicle_component",
                newName: "IX_vehicle_component_ProductionStationId");

            migrationBuilder.RenameIndex(
                name: "IX_kit_component_KitId_ComponentId_ProductionStationId",
                table: "vehicle_component",
                newName: "IX_vehicle_component_KitId_ComponentId_ProductionStationId");

            migrationBuilder.RenameIndex(
                name: "IX_kit_component_ComponentId",
                table: "vehicle_component",
                newName: "IX_vehicle_component_ComponentId");

            migrationBuilder.RenameIndex(
                name: "IX_kit_VIN",
                table: "vehicle",
                newName: "IX_vehicle_VIN");

            migrationBuilder.RenameIndex(
                name: "IX_kit_ModelId",
                table: "vehicle",
                newName: "IX_vehicle_ModelId");

            migrationBuilder.RenameIndex(
                name: "IX_kit_LotId",
                table: "vehicle",
                newName: "IX_vehicle_LotId");

            migrationBuilder.RenameIndex(
                name: "IX_kit_KitNo",
                table: "vehicle",
                newName: "IX_vehicle_KitNo");

            migrationBuilder.AddPrimaryKey(
                name: "PK_vehicle_lot",
                table: "vehicle_lot",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_vehicle_timeline_event_type",
                table: "vehicle_timeline_event_type",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_vehicle_timeline_event",
                table: "vehicle_timeline_event",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_vehicle_snapshot_run",
                table: "vehicle_snapshot_run",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_vehicle_snapshot",
                table: "vehicle_snapshot",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_vehicle_component",
                table: "vehicle_component",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_vehicle",
                table: "vehicle",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_component_serial_vehicle_component_VehicleComponentId",
                table: "component_serial",
                column: "VehicleComponentId",
                principalTable: "vehicle_component",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_lot_part_vehicle_lot_LotId",
                table: "lot_part",
                column: "LotId",
                principalTable: "vehicle_lot",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_vehicle_vehicle_lot_LotId",
                table: "vehicle",
                column: "LotId",
                principalTable: "vehicle_lot",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_vehicle_vehicle_model_ModelId",
                table: "vehicle",
                column: "ModelId",
                principalTable: "vehicle_model",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_vehicle_component_component_ComponentId",
                table: "vehicle_component",
                column: "ComponentId",
                principalTable: "component",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_vehicle_component_production_station_ProductionStationId",
                table: "vehicle_component",
                column: "ProductionStationId",
                principalTable: "production_station",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_vehicle_component_vehicle_KitId",
                table: "vehicle_component",
                column: "KitId",
                principalTable: "vehicle",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_vehicle_lot_bom_BomId",
                table: "vehicle_lot",
                column: "BomId",
                principalTable: "bom",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_vehicle_lot_plant_PlantId",
                table: "vehicle_lot",
                column: "PlantId",
                principalTable: "plant",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_vehicle_snapshot_vehicle_KitId",
                table: "vehicle_snapshot",
                column: "KitId",
                principalTable: "vehicle",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_vehicle_snapshot_vehicle_snapshot_run_VehicleSnapshotRunId",
                table: "vehicle_snapshot",
                column: "VehicleSnapshotRunId",
                principalTable: "vehicle_snapshot_run",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_vehicle_snapshot_run_plant_PlantId",
                table: "vehicle_snapshot_run",
                column: "PlantId",
                principalTable: "plant",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_vehicle_timeline_event_vehicle_KitId",
                table: "vehicle_timeline_event",
                column: "KitId",
                principalTable: "vehicle",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_vehicle_timeline_event_vehicle_timeline_event_type_KitTimelineEventTypeId",
                table: "vehicle_timeline_event",
                column: "KitTimelineEventTypeId",
                principalTable: "vehicle_timeline_event_type",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
