using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;


namespace VT.Model {
    public class AppDbContext : DbContext {
        public DbSet<User> Users { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<Component> Components { get; set; }
        public DbSet<VehicleModel> VehicleModels { get; set; }
        public DbSet<VehicleComponent> VehicleComponents { get; set; }
        public DbSet<VehicleModelComponent> VehicleModelComponents { get; set; }
        
        public AppDbContext(DbContextOptions options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder) {
            builder.ApplyConfiguration(new Component_Config());
            builder.ApplyConfiguration(new User_Config());
            builder.ApplyConfiguration(new Vehicle_Config());
            builder.ApplyConfiguration(new VehicleModel_Config());
            builder.ApplyConfiguration(new VehicleComponent_Config());
            builder.ApplyConfiguration(new VehicleModelComponent_Config());
        }
    }
}