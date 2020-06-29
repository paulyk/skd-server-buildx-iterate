
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SKD.Model {
    public class VehicleComponent_Config : IEntityTypeConfiguration<VehicleComponent> {
        public void Configure(EntityTypeBuilder<VehicleComponent> builder) {

            builder.ToTable("vehicle_component");

            builder.HasKey(t => t.Id);
            builder.Property(t => t.Id).HasMaxLength(EntityMaxLen.Id).ValueGeneratedOnAdd();

            builder.HasIndex(t => new { t.VehicleId, t.ComponentId }).IsUnique();
            builder.HasIndex(t => t.Scan1);
            builder.HasIndex(t => t.Scan2);

            builder.Property(t => t.Scan1).HasMaxLength(EntityMaxLen.VehicleComponent_Scan1);
            builder.Property(t => t.Scan2).HasMaxLength(EntityMaxLen.VehicleComponent_Scan2);
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