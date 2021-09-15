using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SKD.Model.src.Migrations
{
    public partial class Dealer : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IconUURL",
                table: "component",
                newName: "IconURL");

            migrationBuilder.AddColumn<Guid>(
                name: "DealerId",
                table: "kit",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "dealer",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RemovedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dealer", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_kit_DealerId",
                table: "kit",
                column: "DealerId");

            migrationBuilder.CreateIndex(
                name: "IX_dealer_Code",
                table: "dealer",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_dealer_Name",
                table: "dealer",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_kit_dealer_DealerId",
                table: "kit",
                column: "DealerId",
                principalTable: "dealer",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_kit_dealer_DealerId",
                table: "kit");

            migrationBuilder.DropTable(
                name: "dealer");

            migrationBuilder.DropIndex(
                name: "IX_kit_DealerId",
                table: "kit");

            migrationBuilder.DropColumn(
                name: "DealerId",
                table: "kit");

            migrationBuilder.RenameColumn(
                name: "IconURL",
                table: "component",
                newName: "IconUURL");
        }
    }
}
