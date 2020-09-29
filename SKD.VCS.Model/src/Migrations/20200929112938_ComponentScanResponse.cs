using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SKD.VCS.Model.src.Migrations
{
    public partial class ComponentScanResponse : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DCWS_ResponseAt",
                table: "component_scan",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DCWS_ResponseCode",
                table: "component_scan",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "VerifiedAt",
                table: "component_scan",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
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
        }
    }
}
