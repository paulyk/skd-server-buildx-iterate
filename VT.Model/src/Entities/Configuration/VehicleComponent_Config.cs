
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace VT.Model {
    public class VehicleComponent_Config : IEntityTypeConfiguration<VehicleComponent> {
        public void Configure(EntityTypeBuilder<VehicleComponent> builder) {

            builder.ToTable("vehicle_component");

            builder.HasKey(t => t.Id);
            builder.Property(t => t.Id).HasMaxLength(EntityMaxLen.Id).ValueGeneratedOnAdd();

            builder.HasIndex(t => new { t.VehicleId, t.ComponentId }).IsUnique();

            builder.Property(t => t.SerialNumber).HasMaxLength(EntityMaxLen.VehicleComponent_SerialNumber);
            builder.Property(t => t.Sequence).IsRequired();

            builder.HasOne(t => t.Component)
                .WithMany(t => t.VehicleComponents)
                .HasForeignKey(t => t.ComponentId);

            builder.HasOne(t => t.Vehicle)
                .WithMany(t => t.VehicleComponents)
                .HasForeignKey(t => t.VehicleId);

        }
    }
}