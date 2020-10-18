using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SKD.VCS.Model.src.Migrations
{
    public partial class dcws_response : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DCWS_ResponseAt",
                table: "component_scan");

            migrationBuilder.DropColumn(
                name: "DCWS_ResponseCode",
                table: "component_scan");

            migrationBuilder.DropColumn(
                name: "VerifiedAt",
                table: "component_scan");

            migrationBuilder.AddColumn<DateTime>(
                name: "AcceptedAt",
                table: "component_scan",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "dcws_response",
                columns: table => new
                {
                    Id = table.Column<Guid>(maxLength: 36, nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    RemovedAt = table.Column<DateTime>(nullable: true),
                    ResponseCode = table.Column<string>(maxLength: 50, nullable: true),
                    ErrorMessage = table.Column<string>(maxLength: 1000, nullable: true),
                    ComponentScanId = table.Column<Guid>(nullable: false),
                    AcceptedAt = table.Column<DateTime>(nullable: true),
                    RejectedAt = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dcws_response", x => x.Id);
                    table.ForeignKey(
                        name: "FK_dcws_response_component_scan_ComponentScanId",
                        column: x => x.ComponentScanId,
                        principalTable: "component_scan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_dcws_response_ComponentScanId",
                table: "dcws_response",
                column: "ComponentScanId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "dcws_response");

            migrationBuilder.DropColumn(
                name: "AcceptedAt",
                table: "component_scan");

            migrationBuilder.AddColumn<DateTime>(
                name: "DCWS_ResponseAt",
                table: "component_scan",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DCWS_ResponseCode",
                table: "component_scan",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "VerifiedAt",
                table: "component_scan",
                type: "datetime2",
                nullable: true);
        }
    }
}
