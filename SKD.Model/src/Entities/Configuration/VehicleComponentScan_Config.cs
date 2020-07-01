using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SKD.Model {
    public class VehicleComponentScan_Config : IEntityTypeConfiguration<VehicleComponentScan> {
      public void Configure(EntityTypeBuilder<VehicleComponentScan> builder) {
            builder.ToTable("vehicle_component_scan");

            builder.HasKey(t => t.Id);
            builder.Property(t => t.Id).HasMaxLength(EntityMaxLen.Id).ValueGeneratedOnAdd();

            builder.HasIndex(t => t.Scan1);
            builder.HasIndex(t => t.Scan2);

            builder.Property(t => t.Scan1).HasMaxLength(EntityMaxLen.VehicleComponent_Scan);
            builder.Property(t => t.Scan2).HasMaxLength(EntityMaxLen.VehicleComponent_Scan);

            builder.HasOne(t => t.VehicleComponent)
                .WithMany(t => t.ComponentScans)
                .HasForeignKey(t => t.VehicleComponentId);
        }
    }
}