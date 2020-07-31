using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SKD.VCS.Model {
    public class VehicleComponentScan_Config : IEntityTypeConfiguration<ComponentScan> {
      public void Configure(EntityTypeBuilder<ComponentScan> builder) {
            builder.ToTable("component_scan");

            builder.HasKey(t => t.Id);
            builder.Property(t => t.Id).HasMaxLength(EntityMaxLen.Id).ValueGeneratedOnAdd();

            builder.HasIndex(t => t.Scan1);
            builder.HasIndex(t => t.Scan2);

            builder.Property(t => t.Scan1).HasMaxLength(EntityMaxLen.ComponentScan_ScanEntry);
            builder.Property(t => t.Scan2).HasMaxLength(EntityMaxLen.ComponentScan_ScanEntry);

            builder.HasOne(t => t.VehicleComponent)
                .WithMany(t => t.ComponentScans)
                .HasForeignKey(t => t.VehicleComponentId);
        }
    }
}