using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SKD.Model.src.Migrations
{
    public partial class LotShipmentLot : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.AddColumn<Guid>(
                name: "LotId",
                table: "shipment_lot",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.Sql(@"
update shipment_lot
    set LotId = TMP.LotId
FROM (
    select 
        shipment_lot.Id as ShipmentLotId,
        lot.Id as LotId
    from shipment_lot
    join lot on lot.LotNo = shipment_lot.LotNo
) as TMP
where shipment_lot.Id = TMP.ShipmentLotId            
            ");


            migrationBuilder.DropIndex(
                name: "IX_shipment_lot_LotNo",
                table: "shipment_lot");

            migrationBuilder.DropColumn(
                name: "LotNo",
                table: "shipment_lot");


            migrationBuilder.CreateIndex(
                name: "IX_shipment_lot_LotId",
                table: "shipment_lot",
                column: "LotId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_shipment_lot_lot_LotId",
                table: "shipment_lot",
                column: "LotId",
                principalTable: "lot",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LotNo",
                table: "shipment_lot",
                type: "nvarchar(15)",
                maxLength: 15,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql(@"
update shipment_lot
set LotNo = TMP.LotNo
from (
    select 
       shipment_lot.Id as ShipmentLotId ,
       lot.LotNo as LotNo
    from  shipment_lot 
    join lot on lot.Id = shipment_lot.LotId

) as TMP
where TMP.ShipmentLotId = shipment_lot.Id 
            ");

            migrationBuilder.DropForeignKey(
                name: "FK_shipment_lot_lot_LotId",
                table: "shipment_lot");

            migrationBuilder.DropIndex(
                name: "IX_shipment_lot_LotId",
                table: "shipment_lot");

            migrationBuilder.DropColumn(
                name: "LotId",
                table: "shipment_lot");


            migrationBuilder.CreateIndex(
                name: "IX_shipment_lot_LotNo",
                table: "shipment_lot",
                column: "LotNo");
        }
    }
}
