using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SKD.Model {
    public class Vehicle_Config : IEntityTypeConfiguration<Vehicle> {
        public void Configure(EntityTypeBuilder<Vehicle> builder) {

            builder.ToTable("vehicle");
                
            builder.HasKey(t => t.Id);
            builder.Property(t => t.Id).HasMaxLength(EntityFieldLen.Id).ValueGeneratedOnAdd();

            builder.HasIndex(t => t.KitNo).IsUnique();
            builder.HasIndex(t => t.VIN);

            builder.Property(t => t.KitNo).IsRequired().HasMaxLength(EntityFieldLen.Vehicle_KitNo);
            builder.Property(t => t.VIN).HasMaxLength(EntityFieldLen.Vehicle_VIN);

            // relationships
            builder.HasOne(t => t.Model)
                .WithMany(t => t.Vehicles)
                .HasForeignKey(t => t.ModelId);

            builder.HasOne(t => t.Lot)
                .WithMany(t => t.Vehicles)
                .HasForeignKey(t => t.LotId);

            builder.HasMany(t => t.VehicleComponents)
                .WithOne(t => t.Vehicle)
                .HasForeignKey(t => t.VehicleId);

            builder.HasMany(t => t.TimelineEvents)
                .WithOne(t => t.Vehicle)
                .HasForeignKey(t => t.VehicleId);

            builder.HasMany(t => t.StatusSnapshots)
                .WithOne(t => t.Vehicle)
                .HasForeignKey(t => t.VehicleId);
        }
    }
}