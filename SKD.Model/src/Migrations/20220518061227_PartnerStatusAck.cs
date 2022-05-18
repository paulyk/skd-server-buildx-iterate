using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SKD.Model.src.Migrations
{
    public partial class PartnerStatusAck : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "partner_status_ack",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    Accepted = table.Column<bool>(type: "bit", nullable: false),
                    TotalProcessed = table.Column<int>(type: "int", nullable: false),
                    TotalAccepted = table.Column<int>(type: "int", nullable: false),
                    TotalRejected = table.Column<int>(type: "int", nullable: false),
                    FileDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    KitSnapshotRunId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RemovedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_partner_status_ack", x => x.Id);
                    table.ForeignKey(
                        name: "FK_partner_status_ack_kit_snapshot_run_KitSnapshotRunId",
                        column: x => x.KitSnapshotRunId,
                        principalTable: "kit_snapshot_run",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_partner_status_ack_KitSnapshotRunId",
                table: "partner_status_ack",
                column: "KitSnapshotRunId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "partner_status_ack");
        }
    }
}
