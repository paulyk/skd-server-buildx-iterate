using Microsoft.EntityFrameworkCore.Migrations;

namespace SKD.VCS.Model.src.Migrations
{
    public partial class ShipmentLotNoUnique : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_shipment_lot_LotNo",
                table: "shipment_lot");

            migrationBuilder.CreateIndex(
                name: "IX_shipment_lot_LotNo",
                table: "shipment_lot",
                column: "LotNo");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_shipment_lot_LotNo",
                table: "shipment_lot");

            migrationBuilder.CreateIndex(
                name: "IX_shipment_lot_LotNo",
                table: "shipment_lot",
                column: "LotNo",
                unique: true);
        }
    }
}
