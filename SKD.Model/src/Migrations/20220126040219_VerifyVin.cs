using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SKD.Model.src.Migrations
{
    public partial class VerifyVin : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "VerifyVIN",
                table: "kit_snapshot",
                type: "datetime2",
                nullable: true);

  migrationBuilder.Sql( 
@"
insert kit_timeline_event_type (
   Id, Code, [Description], Sequence, CreatedAt
) values (
   newId(), 'VERIFY_VIN', 'Verify VIN before build start',  3, getDate()
)

update kit_timeline_event_type 
set [Sequence] = 1
where Code = 'CUSTOM_RECEIVED'

update kit_timeline_event_type 
set [Sequence] = 2
where Code = 'PLAN_BUILD'

update kit_timeline_event_type 
set [Sequence] = 3
where Code = 'VERIFY_VIN'

update kit_timeline_event_type 
set [Sequence] = 4
where Code = 'BUILD_COMPLETED'

update kit_timeline_event_type 
set [Sequence] = 5
where Code = 'GATE_RELEASED'

update kit_timeline_event_type 
set [Sequence] = 6
where Code = 'WHOLE_SALE'
");                                
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VerifyVIN",
                table: "kit_snapshot");

        migrationBuilder.Sql( 
@"
delete from kit_timeline_event_type where code = 'VERIFY_VIN'

update kit_timeline_event_type 
set [Sequence] = 3
where Code = 'BUILD_COMPLETED'

update kit_timeline_event_type 
set [Sequence] = 4
where Code = 'GATE_RELEASED'

update kit_timeline_event_type 
set [Sequence] = 5
where Code = 'WHOLE_SALE'

update kit_timeline_event_type
set [Description] = 'a.k.a. BUILD START by Ford'
where Code = ''
");                 
        }
    }
}
