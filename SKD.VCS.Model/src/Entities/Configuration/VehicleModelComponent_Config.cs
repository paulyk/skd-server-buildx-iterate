using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SKD.VCS.Model {
    public class VehicleModelComponent_Config : IEntityTypeConfiguration<VehicleModelComponent> {
        public void Configure(EntityTypeBuilder<VehicleModelComponent> builder) {

            builder.ToTable("vehicle_model_component");

            builder.HasKey(t => t.Id);
            builder.HasIndex(t => new { t.VehicleModelId, t.ComponentId, t.ProductionStationId }).IsUnique();

            builder.Property(t => t.Id).HasMaxLength(EntityMaxLen.Id).ValueGeneratedOnAdd();

            builder.HasOne(t => t.VehicleModel)
                .WithMany(t => t.ModelComponents)
                .HasForeignKey(t => t.VehicleModelId);

            builder.HasOne(t => t.Component)
                .WithMany(t => t.VehicleModelComponents)
                .HasForeignKey(t => t.ComponentId);

        }
    }
}