using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SKD.Model.src.Migrations
{
    public partial class AppSetting : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "VIN",
                table: "kit",
                type: "nvarchar(17)",
                maxLength: 17,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(17)",
                oldMaxLength: 17,
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "app_setting",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RemovedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_app_setting", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_app_setting_Code",
                table: "app_setting",
                column: "Code",
                unique: true);

            migrationBuilder.Sql(@"
insert app_setting (id, code, [value], [description], createdAt)  values
(newid(), 'PlanBuildLeadTimeDays', '6', 'Minimum gap in days between custom received and plan build', getDate() )

insert app_setting (id, code, [value], [description], createdAt)  values
(newid(), 'WholeSaleCutoffDays', '7', 'Gap in days after kit marked as wholesale, to continue to include in kit snapshots', getDate() )

insert app_setting (id, code, [value], [description], createdAt)  values
(newid(), 'VerifyVinLeadTimeDays', '2', 'Days prior to plan build to submit Verify VIN', getDate() )

insert app_setting (id, code, [value], [description], createdAt)  values
(newid(), 'EngineComponentCode', 'EN', 'Engine component type code used to retrieve engine serial', getDate() )
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "app_setting");

            migrationBuilder.AlterColumn<string>(
                name: "VIN",
                table: "kit",
                type: "nvarchar(17)",
                maxLength: 17,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(17)",
                oldMaxLength: 17);
        }
    }
}
