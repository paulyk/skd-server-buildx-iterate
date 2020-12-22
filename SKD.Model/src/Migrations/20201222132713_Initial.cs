using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SKD.Model.src.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                name: "part",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    PartNo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    PartDesc = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RemovedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_part", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "plant",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RemovedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_plant", x => x.Id);
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
                name: "vehicle_timeline_event_type",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Sequecne = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RemovedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vehicle_timeline_event_type", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "bom",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    PlantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Sequence = table.Column<int>(type: "int", maxLength: 4, nullable: false),
                    LotPartQuantitiesMatchShipment = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RemovedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bom", x => x.Id);
                    table.ForeignKey(
                        name: "FK_bom_plant_PlantId",
                        column: x => x.PlantId,
                        principalTable: "plant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "shipment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    PlantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RemovedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shipment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_shipment_plant_PlantId",
                        column: x => x.PlantId,
                        principalTable: "plant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "vehicle_snapshot_run",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    PlantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RunDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RemovedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vehicle_snapshot_run", x => x.Id);
                    table.ForeignKey(
                        name: "FK_vehicle_snapshot_run_plant_PlantId",
                        column: x => x.PlantId,
                        principalTable: "plant",
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
                name: "vehicle_lot",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    LotNo = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    PlantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BomId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RemovedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vehicle_lot", x => x.Id);
                    table.ForeignKey(
                        name: "FK_vehicle_lot_bom_BomId",
                        column: x => x.BomId,
                        principalTable: "bom",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_vehicle_lot_plant_PlantId",
                        column: x => x.PlantId,
                        principalTable: "plant",
                        principalColumn: "Id");
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
                name: "lot_part",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    PartId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    LotId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RemovedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lot_part", x => x.Id);
                    table.ForeignKey(
                        name: "FK_lot_part_part_PartId",
                        column: x => x.PartId,
                        principalTable: "part",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_lot_part_vehicle_lot_LotId",
                        column: x => x.LotId,
                        principalTable: "vehicle_lot",
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
                name: "vehicle_snapshot",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    VehicleSnapshotRunId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VehicleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChangeStatusCode = table.Column<int>(type: "int", nullable: false),
                    TimelineEventCode = table.Column<int>(type: "int", nullable: false),
                    VIN = table.Column<string>(type: "nvarchar(17)", maxLength: 17, nullable: true),
                    DealerCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EngineSerialNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CustomReceived = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PlanBuild = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OrginalPlanBuild = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BuildCompleted = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GateRelease = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Wholesale = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RemovedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vehicle_snapshot", x => x.Id);
                    table.ForeignKey(
                        name: "FK_vehicle_snapshot_vehicle_snapshot_run_VehicleSnapshotRunId",
                        column: x => x.VehicleSnapshotRunId,
                        principalTable: "vehicle_snapshot_run",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_vehicle_snapshot_vehicle_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "vehicle",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "vehicle_timeline_event",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    VehicleTimelineEventTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EventNote = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    VehicleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RemovedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vehicle_timeline_event", x => x.Id);
                    table.ForeignKey(
                        name: "FK_vehicle_timeline_event_vehicle_timeline_event_type_VehicleTimelineEventTypeId",
                        column: x => x.VehicleTimelineEventTypeId,
                        principalTable: "vehicle_timeline_event_type",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_vehicle_timeline_event_vehicle_VehicleId",
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
                    PartId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    ShipmentInvoiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RemovedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shipment_part", x => x.Id);
                    table.ForeignKey(
                        name: "FK_shipment_part_part_PartId",
                        column: x => x.PartId,
                        principalTable: "part",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                name: "IX_bom_PlantId_Sequence",
                table: "bom",
                columns: new[] { "PlantId", "Sequence" },
                unique: true);

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
                name: "IX_lot_part_LotId_PartId",
                table: "lot_part",
                columns: new[] { "LotId", "PartId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_lot_part_PartId",
                table: "lot_part",
                column: "PartId");

            migrationBuilder.CreateIndex(
                name: "IX_part_PartDesc",
                table: "part",
                column: "PartDesc",
                unique: true,
                filter: "[PartDesc] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_part_PartNo",
                table: "part",
                column: "PartNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_plant_Code",
                table: "plant",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_plant_Name",
                table: "plant",
                column: "Name",
                unique: true,
                filter: "[Name] IS NOT NULL");

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
                name: "IX_shipment_PlantId_Sequence",
                table: "shipment",
                columns: new[] { "PlantId", "Sequence" },
                unique: true);

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
                name: "IX_shipment_part_PartId",
                table: "shipment_part",
                column: "PartId");

            migrationBuilder.CreateIndex(
                name: "IX_shipment_part_ShipmentInvoiceId_PartId",
                table: "shipment_part",
                columns: new[] { "ShipmentInvoiceId", "PartId" },
                unique: true);

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
                name: "IX_vehicle_lot_BomId",
                table: "vehicle_lot",
                column: "BomId");

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_lot_LotNo",
                table: "vehicle_lot",
                column: "LotNo",
                unique: true,
                filter: "[LotNo] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_lot_PlantId",
                table: "vehicle_lot",
                column: "PlantId");

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
                name: "IX_vehicle_snapshot_VehicleId",
                table: "vehicle_snapshot",
                column: "VehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_snapshot_VehicleSnapshotRunId_VehicleId",
                table: "vehicle_snapshot",
                columns: new[] { "VehicleSnapshotRunId", "VehicleId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_snapshot_run_PlantId_RunDate",
                table: "vehicle_snapshot_run",
                columns: new[] { "PlantId", "RunDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_snapshot_run_PlantId_Sequence",
                table: "vehicle_snapshot_run",
                columns: new[] { "PlantId", "Sequence" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_timeline_event_CreatedAt",
                table: "vehicle_timeline_event",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_timeline_event_VehicleId",
                table: "vehicle_timeline_event",
                column: "VehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_timeline_event_VehicleTimelineEventTypeId",
                table: "vehicle_timeline_event",
                column: "VehicleTimelineEventTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_timeline_event_type_Code",
                table: "vehicle_timeline_event_type",
                column: "Code",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "dcws_response");

            migrationBuilder.DropTable(
                name: "lot_part");

            migrationBuilder.DropTable(
                name: "shipment_part");

            migrationBuilder.DropTable(
                name: "user");

            migrationBuilder.DropTable(
                name: "vehicle_model_component");

            migrationBuilder.DropTable(
                name: "vehicle_snapshot");

            migrationBuilder.DropTable(
                name: "vehicle_timeline_event");

            migrationBuilder.DropTable(
                name: "component_scan");

            migrationBuilder.DropTable(
                name: "part");

            migrationBuilder.DropTable(
                name: "shipment_invoice");

            migrationBuilder.DropTable(
                name: "vehicle_snapshot_run");

            migrationBuilder.DropTable(
                name: "vehicle_timeline_event_type");

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

            migrationBuilder.DropTable(
                name: "bom");

            migrationBuilder.DropTable(
                name: "plant");
        }
    }
}
