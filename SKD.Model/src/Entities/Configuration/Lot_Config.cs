using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SKD.Model {
    public class Lot_Config : IEntityTypeConfiguration<Lot> {
        public void Configure(EntityTypeBuilder<Lot> builder) {

            builder.ToTable("lot");

            builder.HasKey(t => t.Id);
            builder.Property(t => t.Id).HasMaxLength(EntityFieldLen.Id).ValueGeneratedOnAdd();

            builder.Property(t => t.LotNo).HasMaxLength(EntityFieldLen.LotNo);
            builder.HasIndex(t => t.LotNo).IsUnique();

            builder.Property(t => t.Note).HasMaxLength(EntityFieldLen.LotNote);

            // relationships            
            builder.HasOne(t => t.Model)
                .WithMany(t => t.Lots)
                .HasForeignKey(t => t.ModelId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(t => t.Kits)
                .WithOne(t => t.Lot)
                .HasForeignKey(t => t.LotId);

            builder.HasMany(t => t.LotParts)
                .WithOne(t => t.Lot)
                .HasForeignKey(t => t.LotId);

            builder.HasOne(t => t.Plant)
                .WithMany(t => t.Lots)
                .HasForeignKey(t => t.PlantId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.Bom)
                .WithMany(t => t.Lots)
                .HasForeignKey(t => t.BomId);

            builder.HasMany(t => t.ShipmentLots)
                .WithOne(t => t.Lot)
                .HasForeignKey(t => t.LotId);
        }
    }
}