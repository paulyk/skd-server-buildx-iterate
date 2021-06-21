using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SKD.Model.src.Migrations
{
    public partial class KitSnapshotMods : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.AddColumn<Guid>(
                name: "KitTimeLineEventTypeId",
                table: "kit_snapshot",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.Sql(@"
update kit_snapshot
set  KitTimeLineEventTypeId = TMP.Id
FROM (    
    select 
    Id,
    case 
        when Code =  'CUSTOM_RECEIVED' then 0
        when Code = 'PLAN_BUILD' then 1
        when Code =  'BUILD_COMPLETED' then 2
        when Code = 'GATE_RELEASED' then 3
        when Code = 'WHOLE_SALE' then 4
    end as Code
    from kit_timeline_event_type
) as TMP
where TMP.Code = kit_snapshot.TimelineEventCode
            ");

            migrationBuilder.DropColumn(
                name: "TimelineEventCode",
                table: "kit_snapshot");

            migrationBuilder.RenameColumn(
                name: "Sequecne",
                table: "kit_timeline_event_type",
                newName: "Sequence");

            migrationBuilder.CreateIndex(
                name: "IX_kit_snapshot_KitTimeLineEventTypeId",
                table: "kit_snapshot",
                column: "KitTimeLineEventTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_kit_snapshot_kit_timeline_event_type_KitTimeLineEventTypeId",
                table: "kit_snapshot",
                column: "KitTimeLineEventTypeId",
                principalTable: "kit_timeline_event_type",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.AddColumn<int>(
                name: "TimelineEventCode",
                table: "kit_snapshot",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(@"
update kit_snapshot
set TimelineEventCode = TMP.Sequence - 1
FROM (
    select Id, Code, Sequence
    from kit_timeline_event_type
) as TMP
where TMP.Id = kit_snapshot.KitTimeLineEventTypeId
            ");

            migrationBuilder.DropForeignKey(
                name: "FK_kit_snapshot_kit_timeline_event_type_KitTimeLineEventTypeId",
                table: "kit_snapshot");

            migrationBuilder.DropIndex(
                name: "IX_kit_snapshot_KitTimeLineEventTypeId",
                table: "kit_snapshot");

            migrationBuilder.DropColumn(
                name: "KitTimeLineEventTypeId",
                table: "kit_snapshot");

            migrationBuilder.RenameColumn(
                name: "Sequence",
                table: "kit_timeline_event_type",
                newName: "Sequecne");

        }
    }
}
