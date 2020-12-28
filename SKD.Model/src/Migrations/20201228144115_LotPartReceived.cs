using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SKD.Model.src.Migrations
{
    public partial class LotPartReceived : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Quantity",
                table: "lot_part",
                newName: "BomQuantity");

            migrationBuilder.AddColumn<int>(
                name: "ShipmentQuantity",
                table: "lot_part",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "lot_part_received",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    LotPartId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RemovedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lot_part_received", x => x.Id);
                    table.ForeignKey(
                        name: "FK_lot_part_received_lot_part_LotPartId",
                        column: x => x.LotPartId,
                        principalTable: "lot_part",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_lot_part_received_LotPartId",
                table: "lot_part_received",
                column: "LotPartId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "lot_part_received");

            migrationBuilder.DropColumn(
                name: "ShipmentQuantity",
                table: "lot_part");

            migrationBuilder.RenameColumn(
                name: "BomQuantity",
                table: "lot_part",
                newName: "Quantity");
        }
    }
}
