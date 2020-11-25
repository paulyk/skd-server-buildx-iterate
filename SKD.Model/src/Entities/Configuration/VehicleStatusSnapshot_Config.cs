using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SKD.Model {
    public class VehicleStatusSnapshot_Config : IEntityTypeConfiguration<VehicleStatusSnapshots> {
        public void Configure(EntityTypeBuilder<VehicleStatusSnapshots> builder) {

            builder.ToTable("vehicle_status_snapshot");

            builder.HasKey(t => t.Id);
            builder.Property(t => t.Id).HasMaxLength(EntityFieldLen.Id).ValueGeneratedOnAdd();

            builder.HasIndex(t => new { t.PlanBuild, t.RunDate });
            builder.HasIndex(t => new { t.RunDate, t.VehicleId }).IsUnique();

            builder.Property(t => t.VIN).HasMaxLength(EntityFieldLen.Vehicle_VIN);
            // relationships
            builder.HasOne(t => t.Vehicle)
                .WithMany(t => t.StatusSnapshots)
                .HasForeignKey(t => t.VehicleId);

            builder.HasOne(t => t.Plant)
                .WithMany(t => t.VehicleStatusSnapshots)
                .HasForeignKey(t => t.PlantId);
        }
    }
}