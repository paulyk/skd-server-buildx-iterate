using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SKD.Model {
    public class ComponentSerial_Config : IEntityTypeConfiguration<ComponentSerial> {
      public void Configure(EntityTypeBuilder<ComponentSerial> builder) {
            builder.ToTable("component_serial");

            builder.HasKey(t => t.Id);
            builder.Property(t => t.Id).HasMaxLength(EntityFieldLen.Id).ValueGeneratedOnAdd();

            builder.HasIndex(t => t.Serial1);
            builder.HasIndex(t => t.Serial2);

            builder.Property(t => t.Serial1).HasMaxLength(EntityFieldLen.ComponentScan_ScanEntry);
            builder.Property(t => t.Serial2).HasMaxLength(EntityFieldLen.ComponentScan_ScanEntry);

            builder.HasOne(t => t.VehicleComponent)
                .WithMany(t => t.ComponentSerials)
                .HasForeignKey(t => t.VehicleComponentId);

            builder.HasMany(t => t.DCWSResponses)
                .WithOne(t => t.ComponentSerial)
                .HasForeignKey(t => t.ComponentScanId);
        }
    }
}