using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SKD.VCS.Model.src.Migrations
{
    public partial class DcwsResponse_Saved : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AcceptedAt",
                table: "dcws_response");

            migrationBuilder.DropColumn(
                name: "RejectedAt",
                table: "dcws_response");

            migrationBuilder.AddColumn<bool>(
                name: "DcwsSuccessfulSave",
                table: "dcws_response",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DcwsSuccessfulSave",
                table: "dcws_response");

            migrationBuilder.AddColumn<DateTime>(
                name: "AcceptedAt",
                table: "dcws_response",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RejectedAt",
                table: "dcws_response",
                type: "datetime2",
                nullable: true);
        }
    }
}
