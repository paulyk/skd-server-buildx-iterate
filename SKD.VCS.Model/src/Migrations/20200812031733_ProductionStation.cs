using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SKD.VCS.Model.src.Migrations
{
    public partial class ProductionStation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "production_station",
                columns: table => new
                {
                    Id = table.Column<Guid>(maxLength: 36, nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    RemovedAt = table.Column<DateTime>(nullable: true),
                    Code = table.Column<string>(maxLength: 100, nullable: false),
                    Name = table.Column<string>(nullable: true),
                    SortOrder = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_production_station", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_production_station_Code",
                table: "production_station",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_production_station_Name",
                table: "production_station",
                column: "Name",
                unique: true,
                filter: "[Name] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "production_station");
        }
    }
}
