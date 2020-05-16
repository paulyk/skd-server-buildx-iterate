
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace VT.Model {
    public class Vehicle_Config : IEntityTypeConfiguration<Vehicle> {
        public void Configure(EntityTypeBuilder<Vehicle> builder) {

            builder.ToTable("vehicle");
                
            builder.HasKey(t => t.Id);

            builder.HasIndex(t => t.VIN).IsUnique();
                            
            builder.Property(t => t.Id).HasMaxLength(EntityMaxLen.Id).ValueGeneratedOnAdd();

            builder.Property(t => t.VIN)
                .IsRequired()
                .HasMaxLength(EntityMaxLen.Vehicle_VIN);

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