using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;


namespace SKD.VCS.Model {
    public class SkdContext : DbContext {
        public DbSet<User> Users { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<VehicleLot> VehicleLots { get; set; }
        public DbSet<Component> Components { get; set; }
        public DbSet<VehicleModel> VehicleModels { get; set; }
        public DbSet<VehicleComponent> VehicleComponents { get; set; }
        public DbSet<ComponentScan> ComponentScans { get; set; }
        public DbSet<DCWSResponse> DCWSResponses { get; set; }
        public DbSet<VehicleModelComponent> VehicleModelComponents { get; set; }
        public DbSet<ProductionStation> ProductionStations { get; set; }
        
        public SkdContext(DbContextOptions options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder) {
            builder.ApplyConfiguration(new Component_Config());
            builder.ApplyConfiguration(new User_Config());
            builder.ApplyConfiguration(new Vehicle_Config());
            builder.ApplyConfiguration(new VehicleLot_Config());
            builder.ApplyConfiguration(new VehicleModel_Config());
            builder.ApplyConfiguration(new VehicleComponent_Config());
            builder.ApplyConfiguration(new VehicleModelComponent_Config());
            builder.ApplyConfiguration(new VehicleComponentScan_Config());
            builder.ApplyConfiguration(new DCWSResponse_Config());
            builder.ApplyConfiguration(new ProductionStation_Config());
        }
    }
}