using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SKD.VCS.Model.src.Migrations
{
    public partial class VehicleTimeline : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "vehicle_timeline",
                columns: table => new
                {
                    Id = table.Column<Guid>(maxLength: 36, nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    RemovedAt = table.Column<DateTime>(nullable: true),
                    CustomReceivedAt = table.Column<DateTime>(nullable: true),
                    PlanBuildAt = table.Column<DateTime>(nullable: true),
                    BuildCompletedAt = table.Column<DateTime>(nullable: true),
                    GateRleaseAt = table.Column<DateTime>(nullable: true),
                    WholeStateAt = table.Column<DateTime>(nullable: true),
                    ModifiedAt = table.Column<DateTime>(nullable: false),
                    VehicleId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vehicle_timeline", x => x.Id);
                    table.ForeignKey(
                        name: "FK_vehicle_timeline_vehicle_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "vehicle",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_timeline_VehicleId",
                table: "vehicle_timeline",
                column: "VehicleId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "vehicle_timeline");
        }
    }
}
