using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SKD.VCS.Model {
    public class Vehicle_Config : IEntityTypeConfiguration<Vehicle> {
        public void Configure(EntityTypeBuilder<Vehicle> builder) {

            builder.ToTable("vehicle");
                
            builder.HasKey(t => t.Id);
            builder.Property(t => t.Id).HasMaxLength(EntityFieldLen.Id).ValueGeneratedOnAdd();

            builder.HasIndex(t => t.VIN).IsUnique();

            builder.Property(t => t.VIN)
                .IsRequired()
                .HasMaxLength(EntityFieldLen.Vehicle_VIN);

            // relationships
            builder.HasOne(t => t.Model)
                .WithMany(t => t.Vehicles)
                .HasForeignKey(t => t.ModelId);

            builder.HasMany(t => t.VehicleComponents)
                .WithOne(t => t.Vehicle)
                .HasForeignKey(t => t.VehicleId);
        }
    }
}