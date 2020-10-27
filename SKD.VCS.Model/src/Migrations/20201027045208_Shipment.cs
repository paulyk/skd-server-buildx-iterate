using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SKD.VCS.Model.src.Migrations
{
    public partial class Shipment : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "shipment",
                columns: table => new
                {
                    Id = table.Column<Guid>(maxLength: 36, nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    RemovedAt = table.Column<DateTime>(nullable: true),
                    SequenceNo = table.Column<string>(maxLength: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shipment", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "shipment_lot",
                columns: table => new
                {
                    Id = table.Column<Guid>(maxLength: 36, nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    RemovedAt = table.Column<DateTime>(nullable: true),
                    LotNo = table.Column<string>(maxLength: 15, nullable: false),
                    ShipmentId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shipment_lot", x => x.Id);
                    table.ForeignKey(
                        name: "FK_shipment_lot_shipment_ShipmentId",
                        column: x => x.ShipmentId,
                        principalTable: "shipment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "shipment_invoice",
                columns: table => new
                {
                    Id = table.Column<Guid>(maxLength: 36, nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    RemovedAt = table.Column<DateTime>(nullable: true),
                    InvoiceNo = table.Column<string>(maxLength: 11, nullable: false),
                    ShipDate = table.Column<DateTime>(nullable: false),
                    ShipmentLotId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shipment_invoice", x => x.Id);
                    table.ForeignKey(
                        name: "FK_shipment_invoice_shipment_lot_ShipmentLotId",
                        column: x => x.ShipmentLotId,
                        principalTable: "shipment_lot",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "shipment_part",
                columns: table => new
                {
                    Id = table.Column<Guid>(maxLength: 36, nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    RemovedAt = table.Column<DateTime>(nullable: true),
                    PartNo = table.Column<string>(maxLength: 30, nullable: false),
                    CustomerPartNo = table.Column<string>(maxLength: 30, nullable: true),
                    CustomerPartDesc = table.Column<string>(maxLength: 30, nullable: false),
                    Quantity = table.Column<int>(nullable: false),
                    ShipmentInvoiceId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shipment_part", x => x.Id);
                    table.ForeignKey(
                        name: "FK_shipment_part_shipment_invoice_ShipmentInvoiceId",
                        column: x => x.ShipmentInvoiceId,
                        principalTable: "shipment_invoice",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_shipment_SequenceNo",
                table: "shipment",
                column: "SequenceNo");

            migrationBuilder.CreateIndex(
                name: "IX_shipment_invoice_InvoiceNo",
                table: "shipment_invoice",
                column: "InvoiceNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_shipment_invoice_ShipmentLotId",
                table: "shipment_invoice",
                column: "ShipmentLotId");

            migrationBuilder.CreateIndex(
                name: "IX_shipment_lot_LotNo",
                table: "shipment_lot",
                column: "LotNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_shipment_lot_ShipmentId",
                table: "shipment_lot",
                column: "ShipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_shipment_part_PartNo",
                table: "shipment_part",
                column: "PartNo");

            migrationBuilder.CreateIndex(
                name: "IX_shipment_part_ShipmentInvoiceId",
                table: "shipment_part",
                column: "ShipmentInvoiceId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "shipment_part");

            migrationBuilder.DropTable(
                name: "shipment_invoice");

            migrationBuilder.DropTable(
                name: "shipment_lot");

            migrationBuilder.DropTable(
                name: "shipment");
        }
    }
}
