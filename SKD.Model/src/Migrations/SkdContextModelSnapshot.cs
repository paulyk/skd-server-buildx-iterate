﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SKD.Model;

namespace SKD.Model.src.Migrations
{
    [DbContext(typeof(SkdContext))]
    partial class SkdContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .UseIdentityColumns()
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "5.0.0");

            modelBuilder.Entity("SKD.Model.BomSummary", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(36)
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<bool>("LotPartQuantitiesMatchShipment")
                        .HasColumnType("bit");

                    b.Property<Guid>("PlantId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime?>("RemovedAt")
                        .HasColumnType("datetime2");

                    b.Property<int>("Sequence")
                        .HasMaxLength(4)
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("PlantId", "Sequence")
                        .IsUnique();

                    b.ToTable("bom_summary");
                });

            modelBuilder.Entity("SKD.Model.BomSummaryPart", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(36)
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("BomSummaryId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("LotNo")
                        .IsRequired()
                        .HasMaxLength(15)
                        .HasColumnType("nvarchar(15)");

                    b.Property<bool>("MatcheShipmentLotPartQuantity")
                        .HasColumnType("bit");

                    b.Property<string>("PartDesc")
                        .IsRequired()
                        .HasMaxLength(34)
                        .HasColumnType("nvarchar(34)");

                    b.Property<string>("PartNo")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("nvarchar(30)");

                    b.Property<int>("Quantity")
                        .HasColumnType("int");

                    b.Property<DateTime?>("RemovedAt")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.HasIndex("BomSummaryId");

                    b.HasIndex("PartNo");

                    b.HasIndex("LotNo", "PartNo")
                        .IsUnique();

                    b.ToTable("bom_summary_part");
                });

            modelBuilder.Entity("SKD.Model.Component", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(36)
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Code")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("nvarchar(10)");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("IconUURL")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<DateTime?>("RemovedAt")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.HasIndex("Code")
                        .IsUnique();

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("component");
                });

            modelBuilder.Entity("SKD.Model.ComponentScan", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(36)
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime?>("AcceptedAt")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("RemovedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Scan1")
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("Scan2")
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<Guid>("VehicleComponentId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("Scan1");

                    b.HasIndex("Scan2");

                    b.HasIndex("VehicleComponentId");

                    b.ToTable("component_scan");
                });

            modelBuilder.Entity("SKD.Model.DCWSResponse", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(36)
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("ComponentScanId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<bool>("DcwsSuccessfulSave")
                        .HasColumnType("bit");

                    b.Property<string>("ErrorMessage")
                        .HasMaxLength(1000)
                        .HasColumnType("nvarchar(1000)");

                    b.Property<DateTime?>("RemovedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("ResponseCode")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.HasKey("Id");

                    b.HasIndex("ComponentScanId");

                    b.ToTable("dcws_response");
                });

            modelBuilder.Entity("SKD.Model.Plant", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(36)
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Code")
                        .IsRequired()
                        .HasMaxLength(5)
                        .HasColumnType("nvarchar(5)");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Name")
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<DateTime?>("RemovedAt")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.HasIndex("Code")
                        .IsUnique();

                    b.HasIndex("Name")
                        .IsUnique()
                        .HasFilter("[Name] IS NOT NULL");

                    b.ToTable("plant");
                });

            modelBuilder.Entity("SKD.Model.ProductionStation", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(36)
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Code")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(450)");

                    b.Property<DateTime?>("RemovedAt")
                        .HasColumnType("datetime2");

                    b.Property<int>("SortOrder")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("Code")
                        .IsUnique();

                    b.HasIndex("Name")
                        .IsUnique()
                        .HasFilter("[Name] IS NOT NULL");

                    b.ToTable("production_station");
                });

            modelBuilder.Entity("SKD.Model.Shipment", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(36)
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<Guid>("PlantId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime?>("RemovedAt")
                        .HasColumnType("datetime2");

                    b.Property<int>("Sequence")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("PlantId", "Sequence")
                        .IsUnique();

                    b.ToTable("shipment");
                });

            modelBuilder.Entity("SKD.Model.ShipmentInvoice", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(36)
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("InvoiceNo")
                        .IsRequired()
                        .HasMaxLength(11)
                        .HasColumnType("nvarchar(11)");

                    b.Property<DateTime?>("RemovedAt")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("ShipDate")
                        .HasColumnType("datetime2");

                    b.Property<Guid>("ShipmentLotId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("InvoiceNo")
                        .IsUnique();

                    b.HasIndex("ShipmentLotId");

                    b.ToTable("shipment_invoice");
                });

            modelBuilder.Entity("SKD.Model.ShipmentLot", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(36)
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("LotNo")
                        .IsRequired()
                        .HasMaxLength(15)
                        .HasColumnType("nvarchar(15)");

                    b.Property<DateTime?>("RemovedAt")
                        .HasColumnType("datetime2");

                    b.Property<Guid>("ShipmentId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("LotNo");

                    b.HasIndex("ShipmentId");

                    b.ToTable("shipment_lot");
                });

            modelBuilder.Entity("SKD.Model.ShipmentPart", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(36)
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("CustomerPartDesc")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("nvarchar(30)");

                    b.Property<string>("CustomerPartNo")
                        .HasMaxLength(30)
                        .HasColumnType("nvarchar(30)");

                    b.Property<string>("PartNo")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("nvarchar(30)");

                    b.Property<int>("Quantity")
                        .HasColumnType("int");

                    b.Property<DateTime?>("RemovedAt")
                        .HasColumnType("datetime2");

                    b.Property<Guid>("ShipmentInvoiceId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("PartNo");

                    b.HasIndex("ShipmentInvoiceId");

                    b.ToTable("shipment_part");
                });

            modelBuilder.Entity("SKD.Model.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(36)
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(320)
                        .HasColumnType("nvarchar(320)");

                    b.Property<DateTime?>("RemovedAt")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.HasIndex("Email")
                        .IsUnique();

                    b.ToTable("user");
                });

            modelBuilder.Entity("SKD.Model.Vehicle", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(36)
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("KitNo")
                        .IsRequired()
                        .HasMaxLength(17)
                        .HasColumnType("nvarchar(17)");

                    b.Property<Guid>("LotId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("ModelId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime?>("RemovedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("VIN")
                        .HasMaxLength(17)
                        .HasColumnType("nvarchar(17)");

                    b.HasKey("Id");

                    b.HasIndex("KitNo")
                        .IsUnique();

                    b.HasIndex("LotId");

                    b.HasIndex("ModelId");

                    b.HasIndex("VIN");

                    b.ToTable("vehicle");
                });

            modelBuilder.Entity("SKD.Model.VehicleComponent", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(36)
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("ComponentId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<Guid>("ProductionStationId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime?>("RemovedAt")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("ScanVerifiedAt")
                        .HasColumnType("datetime2");

                    b.Property<Guid>("VehicleId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("ComponentId");

                    b.HasIndex("ProductionStationId");

                    b.HasIndex("VehicleId", "ComponentId", "ProductionStationId")
                        .IsUnique();

                    b.ToTable("vehicle_component");
                });

            modelBuilder.Entity("SKD.Model.VehicleLot", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(36)
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("LotNo")
                        .HasMaxLength(15)
                        .HasColumnType("nvarchar(15)");

                    b.Property<Guid>("PlantId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime?>("RemovedAt")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.HasIndex("LotNo")
                        .IsUnique()
                        .HasFilter("[LotNo] IS NOT NULL");

                    b.HasIndex("PlantId");

                    b.ToTable("vehicle_lot");
                });

            modelBuilder.Entity("SKD.Model.VehicleModel", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(36)
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Code")
                        .IsRequired()
                        .HasMaxLength(11)
                        .HasColumnType("nvarchar(11)");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<DateTime?>("RemovedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Type")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("Code")
                        .IsUnique();

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("vehicle_model");
                });

            modelBuilder.Entity("SKD.Model.VehicleModelComponent", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(36)
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("ComponentId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<Guid>("ProductionStationId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime?>("RemovedAt")
                        .HasColumnType("datetime2");

                    b.Property<Guid>("VehicleModelId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("ComponentId");

                    b.HasIndex("ProductionStationId");

                    b.HasIndex("VehicleModelId", "ComponentId", "ProductionStationId")
                        .IsUnique();

                    b.ToTable("vehicle_model_component");
                });

            modelBuilder.Entity("SKD.Model.VehicleSnapshot", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(36)
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime?>("BuildCompleted")
                        .HasColumnType("datetime2");

                    b.Property<int>("ChangeStatusCode")
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("CustomReceived")
                        .HasColumnType("datetime2");

                    b.Property<string>("DealerCode")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("EngineSerialNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("GateRelease")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("OrginalPlanBuild")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("PlanBuild")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("RemovedAt")
                        .HasColumnType("datetime2");

                    b.Property<int>("TimelineEventCode")
                        .HasColumnType("int");

                    b.Property<string>("VIN")
                        .HasMaxLength(17)
                        .HasColumnType("nvarchar(17)");

                    b.Property<Guid>("VehicleId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("VehicleSnapshotRunId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime?>("Wholesale")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.HasIndex("VehicleId");

                    b.HasIndex("VehicleSnapshotRunId", "VehicleId")
                        .IsUnique();

                    b.ToTable("vehicle_snapshot");
                });

            modelBuilder.Entity("SKD.Model.VehicleSnapshotRun", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(36)
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<Guid>("PlantId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime?>("RemovedAt")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("RunDate")
                        .HasColumnType("datetime2");

                    b.Property<int>("Sequence")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("PlantId", "RunDate")
                        .IsUnique();

                    b.HasIndex("PlantId", "Sequence")
                        .IsUnique();

                    b.ToTable("vehicle_snapshot_run");
                });

            modelBuilder.Entity("SKD.Model.VehicleTimelineEvent", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(36)
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("EventDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("EventNote")
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<DateTime?>("RemovedAt")
                        .HasColumnType("datetime2");

                    b.Property<Guid>("VehicleId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("VehicleTimelineEventTypeId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("CreatedAt");

                    b.HasIndex("VehicleId");

                    b.HasIndex("VehicleTimelineEventTypeId");

                    b.ToTable("vehicle_timeline_event");
                });

            modelBuilder.Entity("SKD.Model.VehicleTimelineEventType", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(36)
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Code")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("nvarchar(30)");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<DateTime?>("RemovedAt")
                        .HasColumnType("datetime2");

                    b.Property<int>("Sequecne")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("Code")
                        .IsUnique();

                    b.ToTable("vehicle_timeline_event_type");
                });

            modelBuilder.Entity("SKD.Model.BomSummary", b =>
                {
                    b.HasOne("SKD.Model.Plant", "Plant")
                        .WithMany("BomSummaries")
                        .HasForeignKey("PlantId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Plant");
                });

            modelBuilder.Entity("SKD.Model.BomSummaryPart", b =>
                {
                    b.HasOne("SKD.Model.BomSummary", "BomSummary")
                        .WithMany("Parts")
                        .HasForeignKey("BomSummaryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("BomSummary");
                });

            modelBuilder.Entity("SKD.Model.ComponentScan", b =>
                {
                    b.HasOne("SKD.Model.VehicleComponent", "VehicleComponent")
                        .WithMany("ComponentScans")
                        .HasForeignKey("VehicleComponentId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("VehicleComponent");
                });

            modelBuilder.Entity("SKD.Model.DCWSResponse", b =>
                {
                    b.HasOne("SKD.Model.ComponentScan", "ComponentScan")
                        .WithMany("DCWSResponses")
                        .HasForeignKey("ComponentScanId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ComponentScan");
                });

            modelBuilder.Entity("SKD.Model.Shipment", b =>
                {
                    b.HasOne("SKD.Model.Plant", "Plant")
                        .WithMany("Shipments")
                        .HasForeignKey("PlantId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Plant");
                });

            modelBuilder.Entity("SKD.Model.ShipmentInvoice", b =>
                {
                    b.HasOne("SKD.Model.ShipmentLot", "ShipmentLot")
                        .WithMany("Invoices")
                        .HasForeignKey("ShipmentLotId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ShipmentLot");
                });

            modelBuilder.Entity("SKD.Model.ShipmentLot", b =>
                {
                    b.HasOne("SKD.Model.Shipment", "Shipment")
                        .WithMany("Lots")
                        .HasForeignKey("ShipmentId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Shipment");
                });

            modelBuilder.Entity("SKD.Model.ShipmentPart", b =>
                {
                    b.HasOne("SKD.Model.ShipmentInvoice", "ShipmentInvoice")
                        .WithMany("Parts")
                        .HasForeignKey("ShipmentInvoiceId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ShipmentInvoice");
                });

            modelBuilder.Entity("SKD.Model.Vehicle", b =>
                {
                    b.HasOne("SKD.Model.VehicleLot", "Lot")
                        .WithMany("Vehicles")
                        .HasForeignKey("LotId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("SKD.Model.VehicleModel", "Model")
                        .WithMany("Vehicles")
                        .HasForeignKey("ModelId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Lot");

                    b.Navigation("Model");
                });

            modelBuilder.Entity("SKD.Model.VehicleComponent", b =>
                {
                    b.HasOne("SKD.Model.Component", "Component")
                        .WithMany("VehicleComponents")
                        .HasForeignKey("ComponentId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("SKD.Model.ProductionStation", "ProductionStation")
                        .WithMany("VehicleComponents")
                        .HasForeignKey("ProductionStationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("SKD.Model.Vehicle", "Vehicle")
                        .WithMany("VehicleComponents")
                        .HasForeignKey("VehicleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Component");

                    b.Navigation("ProductionStation");

                    b.Navigation("Vehicle");
                });

            modelBuilder.Entity("SKD.Model.VehicleLot", b =>
                {
                    b.HasOne("SKD.Model.Plant", "Plant")
                        .WithMany("VehicleLots")
                        .HasForeignKey("PlantId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Plant");
                });

            modelBuilder.Entity("SKD.Model.VehicleModelComponent", b =>
                {
                    b.HasOne("SKD.Model.Component", "Component")
                        .WithMany("VehicleModelComponents")
                        .HasForeignKey("ComponentId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("SKD.Model.ProductionStation", "ProductionStation")
                        .WithMany("ModelComponents")
                        .HasForeignKey("ProductionStationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("SKD.Model.VehicleModel", "VehicleModel")
                        .WithMany("ModelComponents")
                        .HasForeignKey("VehicleModelId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Component");

                    b.Navigation("ProductionStation");

                    b.Navigation("VehicleModel");
                });

            modelBuilder.Entity("SKD.Model.VehicleSnapshot", b =>
                {
                    b.HasOne("SKD.Model.Vehicle", "Vehicle")
                        .WithMany("Snapshots")
                        .HasForeignKey("VehicleId")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.HasOne("SKD.Model.VehicleSnapshotRun", "VehicleSnapshotRun")
                        .WithMany("VehicleSnapshots")
                        .HasForeignKey("VehicleSnapshotRunId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Vehicle");

                    b.Navigation("VehicleSnapshotRun");
                });

            modelBuilder.Entity("SKD.Model.VehicleSnapshotRun", b =>
                {
                    b.HasOne("SKD.Model.Plant", "Plant")
                        .WithMany("VehicleSnapshotRuns")
                        .HasForeignKey("PlantId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Plant");
                });

            modelBuilder.Entity("SKD.Model.VehicleTimelineEvent", b =>
                {
                    b.HasOne("SKD.Model.Vehicle", "Vehicle")
                        .WithMany("TimelineEvents")
                        .HasForeignKey("VehicleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("SKD.Model.VehicleTimelineEventType", "EventType")
                        .WithMany()
                        .HasForeignKey("VehicleTimelineEventTypeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("EventType");

                    b.Navigation("Vehicle");
                });

            modelBuilder.Entity("SKD.Model.BomSummary", b =>
                {
                    b.Navigation("Parts");
                });

            modelBuilder.Entity("SKD.Model.Component", b =>
                {
                    b.Navigation("VehicleComponents");

                    b.Navigation("VehicleModelComponents");
                });

            modelBuilder.Entity("SKD.Model.ComponentScan", b =>
                {
                    b.Navigation("DCWSResponses");
                });

            modelBuilder.Entity("SKD.Model.Plant", b =>
                {
                    b.Navigation("BomSummaries");

                    b.Navigation("Shipments");

                    b.Navigation("VehicleLots");

                    b.Navigation("VehicleSnapshotRuns");
                });

            modelBuilder.Entity("SKD.Model.ProductionStation", b =>
                {
                    b.Navigation("ModelComponents");

                    b.Navigation("VehicleComponents");
                });

            modelBuilder.Entity("SKD.Model.Shipment", b =>
                {
                    b.Navigation("Lots");
                });

            modelBuilder.Entity("SKD.Model.ShipmentInvoice", b =>
                {
                    b.Navigation("Parts");
                });

            modelBuilder.Entity("SKD.Model.ShipmentLot", b =>
                {
                    b.Navigation("Invoices");
                });

            modelBuilder.Entity("SKD.Model.Vehicle", b =>
                {
                    b.Navigation("Snapshots");

                    b.Navigation("TimelineEvents");

                    b.Navigation("VehicleComponents");
                });

            modelBuilder.Entity("SKD.Model.VehicleComponent", b =>
                {
                    b.Navigation("ComponentScans");
                });

            modelBuilder.Entity("SKD.Model.VehicleLot", b =>
                {
                    b.Navigation("Vehicles");
                });

            modelBuilder.Entity("SKD.Model.VehicleModel", b =>
                {
                    b.Navigation("ModelComponents");

                    b.Navigation("Vehicles");
                });

            modelBuilder.Entity("SKD.Model.VehicleSnapshotRun", b =>
                {
                    b.Navigation("VehicleSnapshots");
                });
#pragma warning restore 612, 618
        }
    }
}
