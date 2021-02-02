using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;


namespace SKD.Model {
    public class SkdContext : DbContext {
        public DbSet<User> Users { get; set; }

        public DbSet<Plant> Plants { get; set; }
        public DbSet<Kit> Kits { get; set; }
        public DbSet<KitTimelineEvent> KitTimelineEvents { get; set; }
        public DbSet<KitTimelineEventType> VehicleTimelineEventTypes { get; set; }
        public DbSet<Lot> Lots { get; set; }
        public DbSet<Component> Components { get; set; }
        public DbSet<VehicleModel> VehicleModels { get; set; }
        public DbSet<KitComponent> VehicleComponents { get; set; }
        public DbSet<ComponentSerial> ComponentSerials { get; set; }
        public DbSet<DcwsResponse> DCWSResponses { get; set; }
        public DbSet<VehicleModelComponent> VehicleModelComponents { get; set; }
        public DbSet<ProductionStation> ProductionStations { get; set; }
        public DbSet<Part> Parts { get; set; }
        
        public DbSet<Shipment> Shipments { get; set; }
        public DbSet<ShipmentLot> ShipmentLots { get; set; }
        public DbSet<ShipmentInvoice> ShipmentInvoices { get; set; }
        public DbSet<ShipmentPart> ShipmentParts { get; set; }

        public DbSet<Bom> Boms { get; set; }
        public DbSet<LotPart> LotParts { get; set; }
        public DbSet<LotPartReceived> LotPartsReceived { get; set; }

        public DbSet<kitSnapshotRun> KitSnapshotRuns { get; set; }
        public DbSet<KitSnapshot> KitSnapshots { get; set; }

        public SkdContext(DbContextOptions options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder) {
            builder.ApplyConfiguration(new Component_Config());
            builder.ApplyConfiguration(new User_Config());
            builder.ApplyConfiguration(new Plant_Config());
            builder.ApplyConfiguration(new Kit_Config());
            builder.ApplyConfiguration(new Lot_Config());
            builder.ApplyConfiguration(new VehicleModel_Config());
            builder.ApplyConfiguration(new KitComponent_Config());
            builder.ApplyConfiguration(new VehicleModelComponent_Config());
            builder.ApplyConfiguration(new ComponentSerial_Config());
            builder.ApplyConfiguration(new KitTimelineEventType_Config());
            builder.ApplyConfiguration(new KitTimelineEvent_Config());
            builder.ApplyConfiguration(new DCWSResponse_Config());
            builder.ApplyConfiguration(new ProductionStation_Config());
            builder.ApplyConfiguration(new Part_Config());
            //
            builder.ApplyConfiguration(new Shipment_Config());
            builder.ApplyConfiguration(new ShipmentLot_Config());
            builder.ApplyConfiguration(new ShipmentInvoice_Config());
            builder.ApplyConfiguration(new ShipmentPart_Config());
            //
            builder.ApplyConfiguration(new Bom_Config());
            builder.ApplyConfiguration(new LotPart_Config());
            builder.ApplyConfiguration(new LotPartReceived_Config());

            builder.ApplyConfiguration(new KitSnapshot_Config());
            builder.ApplyConfiguration(new KitSnapshotRun_Config());
        }
    }
}