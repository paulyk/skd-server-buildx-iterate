using Microsoft.EntityFrameworkCore.Migrations;

namespace SKD.Model.src.Migrations
{
    public partial class Rename : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_vehicle_component_vehicle_VehicleId",
                table: "vehicle_component");

            migrationBuilder.DropForeignKey(
                name: "FK_vehicle_snapshot_vehicle_VehicleId",
                table: "vehicle_snapshot");

            migrationBuilder.DropForeignKey(
                name: "FK_vehicle_timeline_event_vehicle_timeline_event_type_VehicleTimelineEventTypeId",
                table: "vehicle_timeline_event");

            migrationBuilder.DropForeignKey(
                name: "FK_vehicle_timeline_event_vehicle_VehicleId",
                table: "vehicle_timeline_event");

            migrationBuilder.RenameColumn(
                name: "VehicleTimelineEventTypeId",
                table: "vehicle_timeline_event",
                newName: "KitTimelineEventTypeId");

            migrationBuilder.RenameColumn(
                name: "VehicleId",
                table: "vehicle_timeline_event",
                newName: "KitId");

            migrationBuilder.RenameIndex(
                name: "IX_vehicle_timeline_event_VehicleTimelineEventTypeId",
                table: "vehicle_timeline_event",
                newName: "IX_vehicle_timeline_event_KitTimelineEventTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_vehicle_timeline_event_VehicleId",
                table: "vehicle_timeline_event",
                newName: "IX_vehicle_timeline_event_KitId");

            migrationBuilder.RenameColumn(
                name: "VehicleId",
                table: "vehicle_snapshot",
                newName: "KitId");

            migrationBuilder.RenameIndex(
                name: "IX_vehicle_snapshot_VehicleSnapshotRunId_VehicleId",
                table: "vehicle_snapshot",
                newName: "IX_vehicle_snapshot_VehicleSnapshotRunId_KitId");

            migrationBuilder.RenameIndex(
                name: "IX_vehicle_snapshot_VehicleId",
                table: "vehicle_snapshot",
                newName: "IX_vehicle_snapshot_KitId");

            migrationBuilder.RenameColumn(
                name: "VehicleId",
                table: "vehicle_component",
                newName: "KitId");

            migrationBuilder.RenameIndex(
                name: "IX_vehicle_component_VehicleId_ComponentId_ProductionStationId",
                table: "vehicle_component",
                newName: "IX_vehicle_component_KitId_ComponentId_ProductionStationId");

            migrationBuilder.AddForeignKey(
                name: "FK_vehicle_component_vehicle_KitId",
                table: "vehicle_component",
                column: "KitId",
                principalTable: "vehicle",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_vehicle_snapshot_vehicle_KitId",
                table: "vehicle_snapshot",
                column: "KitId",
                principalTable: "vehicle",
                principalColumn: "Id");

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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_vehicle_component_vehicle_KitId",
                table: "vehicle_component");

            migrationBuilder.DropForeignKey(
                name: "FK_vehicle_snapshot_vehicle_KitId",
                table: "vehicle_snapshot");

            migrationBuilder.DropForeignKey(
                name: "FK_vehicle_timeline_event_vehicle_KitId",
                table: "vehicle_timeline_event");

            migrationBuilder.DropForeignKey(
                name: "FK_vehicle_timeline_event_vehicle_timeline_event_type_KitTimelineEventTypeId",
                table: "vehicle_timeline_event");

            migrationBuilder.RenameColumn(
                name: "KitTimelineEventTypeId",
                table: "vehicle_timeline_event",
                newName: "VehicleTimelineEventTypeId");

            migrationBuilder.RenameColumn(
                name: "KitId",
                table: "vehicle_timeline_event",
                newName: "VehicleId");

            migrationBuilder.RenameIndex(
                name: "IX_vehicle_timeline_event_KitTimelineEventTypeId",
                table: "vehicle_timeline_event",
                newName: "IX_vehicle_timeline_event_VehicleTimelineEventTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_vehicle_timeline_event_KitId",
                table: "vehicle_timeline_event",
                newName: "IX_vehicle_timeline_event_VehicleId");

            migrationBuilder.RenameColumn(
                name: "KitId",
                table: "vehicle_snapshot",
                newName: "VehicleId");

            migrationBuilder.RenameIndex(
                name: "IX_vehicle_snapshot_VehicleSnapshotRunId_KitId",
                table: "vehicle_snapshot",
                newName: "IX_vehicle_snapshot_VehicleSnapshotRunId_VehicleId");

            migrationBuilder.RenameIndex(
                name: "IX_vehicle_snapshot_KitId",
                table: "vehicle_snapshot",
                newName: "IX_vehicle_snapshot_VehicleId");

            migrationBuilder.RenameColumn(
                name: "KitId",
                table: "vehicle_component",
                newName: "VehicleId");

            migrationBuilder.RenameIndex(
                name: "IX_vehicle_component_KitId_ComponentId_ProductionStationId",
                table: "vehicle_component",
                newName: "IX_vehicle_component_VehicleId_ComponentId_ProductionStationId");

            migrationBuilder.AddForeignKey(
                name: "FK_vehicle_component_vehicle_VehicleId",
                table: "vehicle_component",
                column: "VehicleId",
                principalTable: "vehicle",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_vehicle_snapshot_vehicle_VehicleId",
                table: "vehicle_snapshot",
                column: "VehicleId",
                principalTable: "vehicle",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_vehicle_timeline_event_vehicle_timeline_event_type_VehicleTimelineEventTypeId",
                table: "vehicle_timeline_event",
                column: "VehicleTimelineEventTypeId",
                principalTable: "vehicle_timeline_event_type",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_vehicle_timeline_event_vehicle_VehicleId",
                table: "vehicle_timeline_event",
                column: "VehicleId",
                principalTable: "vehicle",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
