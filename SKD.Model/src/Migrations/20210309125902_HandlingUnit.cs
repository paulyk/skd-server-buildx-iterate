using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SKD.Model.src.Migrations
{
    public partial class HandlingUnit : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_shipment_part_shipment_invoice_ShipmentInvoiceId",
                table: "shipment_part");

            migrationBuilder.DropIndex(
                name: "IX_shipment_part_ShipmentInvoiceId_PartId",
                table: "shipment_part");


            migrationBuilder.AddColumn<Guid>(
                name: "HandlingUnitId",
                table: "shipment_part",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "handling_unit",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: true),
                    ShipmentInvoiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RemovedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_handling_unit", x => x.Id);
                    table.ForeignKey(
                        name: "FK_handling_unit_shipment_invoice_ShipmentInvoiceId",
                        column: x => x.ShipmentInvoiceId,
                        principalTable: "shipment_invoice",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // add one handling_unit for each shipment_invoice
            migrationBuilder.Sql(@"
insert into handling_unit 
    (ID, Code, ShipmentInvoiceId, CreatedAt)
select 
    NEWID(),
    FORMAT((ROW_NUMBER() OVER(ORDER BY ID))  , '0000000') as Code,  
    Id,
    getdate()
from shipment_invoice
            ");

            // link shipment_part with handling_unit
            migrationBuilder.Sql(@"
update shipment_part
set HandlingUnitId = TMP.Id
FROM (
    select Id, ShipmentInvoiceId
    from handling_unit
) as TMP
where TMP.ShipmentInvoiceId = shipment_part.ShipmentInvoiceId
            ");

            migrationBuilder.DropColumn(
                name: "ShipmentInvoiceId",
                table: "shipment_part");

            migrationBuilder.CreateIndex(
                name: "IX_shipment_part_HandlingUnitId_PartId",
                table: "shipment_part",
                columns: new[] { "HandlingUnitId", "PartId" },
                unique: true,
                filter: "[HandlingUnitId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_handling_unit_Code",
                table: "handling_unit",
                column: "Code",
                unique: true,
                filter: "[Code] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_handling_unit_ShipmentInvoiceId",
                table: "handling_unit",
                column: "ShipmentInvoiceId");

            migrationBuilder.AddForeignKey(
                name: "FK_shipment_part_handling_unit_HandlingUnitId",
                table: "shipment_part",
                column: "HandlingUnitId",
                principalTable: "handling_unit",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.AddColumn<Guid>(
                name: "ShipmentInvoiceId",
                table: "shipment_part",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            // link shipment_part to shipment_invoice
            migrationBuilder.Sql(@"
update shipment_part
set ShipmentInvoiceId = TMP.ShipmentInvoiceId
FROM (
    select Id, ShipmentInvoiceId
    from handling_unit
) as TMP
where TMP.Id = shipment_part.HandlingUnitId            
            
            ");

            migrationBuilder.DropForeignKey(
                name: "FK_shipment_part_handling_unit_HandlingUnitId",
                table: "shipment_part");

            migrationBuilder.DropTable(
                name: "handling_unit");

            migrationBuilder.DropIndex(
                name: "IX_shipment_part_HandlingUnitId_PartId",
                table: "shipment_part");

            migrationBuilder.DropColumn(
                name: "HandlingUnitId",
                table: "shipment_part");

            migrationBuilder.CreateIndex(
                name: "IX_shipment_part_ShipmentInvoiceId_PartId",
                table: "shipment_part",
                columns: new[] { "ShipmentInvoiceId", "PartId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_shipment_part_shipment_invoice_ShipmentInvoiceId",
                table: "shipment_part",
                column: "ShipmentInvoiceId",
                principalTable: "shipment_invoice",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
