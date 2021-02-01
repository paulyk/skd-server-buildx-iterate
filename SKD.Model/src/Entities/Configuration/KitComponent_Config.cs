
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SKD.Model {
    public class KitComponent_Config : IEntityTypeConfiguration<KitComponent> {
        public void Configure(EntityTypeBuilder<KitComponent> builder) {

            builder.ToTable("vehicle_component");

            builder.HasKey(t => t.Id);
            builder.Property(t => t.Id).HasMaxLength(EntityFieldLen.Id).ValueGeneratedOnAdd();

            builder.HasIndex(t => new { t.KitId, t.ComponentId, t.ProductionStationId }).IsUnique();            

            builder.HasOne(t => t.Component)
                .WithMany(t => t.KitComponents)
                .HasForeignKey(t => t.ComponentId);

            builder.HasOne(t => t.Kit)
                .WithMany(t => t.KitComponents)
                .HasForeignKey(t => t.KitId);

            builder.HasMany(t => t.ComponentSerials)
                .WithOne(t => t.VehicleComponent)
                .HasForeignKey(t => t.VehicleComponentId);

        }
    }
}