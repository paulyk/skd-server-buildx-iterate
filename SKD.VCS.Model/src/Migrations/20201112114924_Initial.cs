using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SKD.VCS.Model.src.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "bom_summary",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    SequenceNo = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    LotPartQuantitiesMatchShipment = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RemovedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bom_summary", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "component",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IconUURL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RemovedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_component", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "production_station",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RemovedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_production_station", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "shipment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    SequenceNo = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RemovedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shipment", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "user",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RemovedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "vehicle_lot",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    LotNo = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RemovedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vehicle_lot", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "vehicle_model",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(11)", maxLength: 11, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RemovedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vehicle_model", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "bom_summary_part",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    LotNo = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    PartNo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    PartDesc = table.Column<string>(type: "nvarchar(34)", maxLength: 34, nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    MatcheShipmentLotPartQuantity = table.Column<bool>(type: "bit", nullable: false),
                    BomSummaryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RemovedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bom_summary_part", x => x.Id);
                    table.ForeignKey(
                        name: "FK_bom_summary_part_bom_summary_BomSummaryId",
                        column: x => x.BomSummaryId,
                        principalTable: "bom_summary",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "shipment_lot",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    LotNo = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    ShipmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RemovedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
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
                name: "vehicle",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    VIN = table.Column<string>(type: "nvarchar(17)", maxLength: 17, nullable: true),
                    KitNo = table.Column<string>(type: "nvarchar(17)", maxLength: 17, nullable: false),
                    ModelId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LotId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RemovedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vehicle", x => x.Id);
                    table.ForeignKey(
                        name: "FK_vehicle_vehicle_lot_LotId",
                        column: x => x.LotId,
                        principalTable: "vehicle_lot",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_vehicle_vehicle_model_ModelId",
                        column: x => x.ModelId,
                        principalTable: "vehicle_model",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "vehicle_model_component",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    VehicleModelId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ComponentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductionStationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RemovedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vehicle_model_component", x => x.Id);
                    table.ForeignKey(
                        name: "FK_vehicle_model_component_component_ComponentId",
                        column: x => x.ComponentId,
                        principalTable: "component",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_vehicle_model_component_production_station_ProductionStationId",
                        column: x => x.ProductionStationId,
                        principalTable: "production_station",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_vehicle_model_component_vehicle_model_VehicleModelId",
                        column: x => x.VehicleModelId,
                        principalTable: "vehicle_model",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "shipment_invoice",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    InvoiceNo = table.Column<string>(type: "nvarchar(11)", maxLength: 11, nullable: false),
                    ShipDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ShipmentLotId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RemovedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
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
                name: "vehicle_component",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    VehicleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ComponentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductionStationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ScanVerifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RemovedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vehicle_component", x => x.Id);
                    table.ForeignKey(
                        name: "FK_vehicle_component_component_ComponentId",
                        column: x => x.ComponentId,
                        principalTable: "component",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_vehicle_component_production_station_ProductionStationId",
                        column: x => x.ProductionStationId,
                        principalTable: "production_station",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_vehicle_component_vehicle_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "vehicle",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "vehicle_timeline",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    CustomReceivedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PlanBuildAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BuildCompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GateRleaseAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    WholeStateAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    VehicleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RemovedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vehicle_timeline", x => x.Id);
                    table.ForeignKey(
                        name: "FK_vehicle_timeline_vehicle_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "vehicle",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "shipment_part",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    PartNo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CustomerPartNo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    CustomerPartDesc = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    ShipmentInvoiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RemovedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
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

            migrationBuilder.CreateTable(
                name: "component_scan",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    VehicleComponentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Scan1 = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Scan2 = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AcceptedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RemovedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_component_scan", x => x.Id);
                    table.ForeignKey(
                        name: "FK_component_scan_vehicle_component_VehicleComponentId",
                        column: x => x.VehicleComponentId,
                        principalTable: "vehicle_component",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "dcws_response",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    ResponseCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ComponentScanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DcwsSuccessfulSave = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RemovedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
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
                name: "IX_bom_summary_SequenceNo",
                table: "bom_summary",
                column: "SequenceNo");

            migrationBuilder.CreateIndex(
                name: "IX_bom_summary_part_BomSummaryId",
                table: "bom_summary_part",
                column: "BomSummaryId");

            migrationBuilder.CreateIndex(
                name: "IX_bom_summary_part_LotNo_PartNo",
                table: "bom_summary_part",
                columns: new[] { "LotNo", "PartNo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_bom_summary_part_PartNo",
                table: "bom_summary_part",
                column: "PartNo");

            migrationBuilder.CreateIndex(
                name: "IX_component_Code",
                table: "component",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_component_Name",
                table: "component",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_component_scan_Scan1",
                table: "component_scan",
                column: "Scan1");

            migrationBuilder.CreateIndex(
                name: "IX_component_scan_Scan2",
                table: "component_scan",
                column: "Scan2");

            migrationBuilder.CreateIndex(
                name: "IX_component_scan_VehicleComponentId",
                table: "component_scan",
                column: "VehicleComponentId");

            migrationBuilder.CreateIndex(
                name: "IX_dcws_response_ComponentScanId",
                table: "dcws_response",
                column: "ComponentScanId");

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
                column: "LotNo");

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

            migrationBuilder.CreateIndex(
                name: "IX_user_Email",
                table: "user",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_KitNo",
                table: "vehicle",
                column: "KitNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_LotId",
                table: "vehicle",
                column: "LotId");

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_ModelId",
                table: "vehicle",
                column: "ModelId");

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_VIN",
                table: "vehicle",
                column: "VIN");

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_component_ComponentId",
                table: "vehicle_component",
                column: "ComponentId");

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_component_ProductionStationId",
                table: "vehicle_component",
                column: "ProductionStationId");

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_component_VehicleId_ComponentId_ProductionStationId",
                table: "vehicle_component",
                columns: new[] { "VehicleId", "ComponentId", "ProductionStationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_lot_LotNo",
                table: "vehicle_lot",
                column: "LotNo",
                unique: true,
                filter: "[LotNo] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_model_Code",
                table: "vehicle_model",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_model_Name",
                table: "vehicle_model",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_model_component_ComponentId",
                table: "vehicle_model_component",
                column: "ComponentId");

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_model_component_ProductionStationId",
                table: "vehicle_model_component",
                column: "ProductionStationId");

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_model_component_VehicleModelId_ComponentId_ProductionStationId",
                table: "vehicle_model_component",
                columns: new[] { "VehicleModelId", "ComponentId", "ProductionStationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_timeline_VehicleId",
                table: "vehicle_timeline",
                column: "VehicleId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bom_summary_part");

            migrationBuilder.DropTable(
                name: "dcws_response");

            migrationBuilder.DropTable(
                name: "shipment_part");

            migrationBuilder.DropTable(
                name: "user");

            migrationBuilder.DropTable(
                name: "vehicle_model_component");

            migrationBuilder.DropTable(
                name: "vehicle_timeline");

            migrationBuilder.DropTable(
                name: "bom_summary");

            migrationBuilder.DropTable(
                name: "component_scan");

            migrationBuilder.DropTable(
                name: "shipment_invoice");

            migrationBuilder.DropTable(
                name: "vehicle_component");

            migrationBuilder.DropTable(
                name: "shipment_lot");

            migrationBuilder.DropTable(
                name: "component");

            migrationBuilder.DropTable(
                name: "production_station");

            migrationBuilder.DropTable(
                name: "vehicle");

            migrationBuilder.DropTable(
                name: "shipment");

            migrationBuilder.DropTable(
                name: "vehicle_lot");

            migrationBuilder.DropTable(
                name: "vehicle_model");
        }
    }
}
