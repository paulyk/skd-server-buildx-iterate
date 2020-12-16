using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SKD.Model {
    public class VehicleLot_Config : IEntityTypeConfiguration<VehicleLot> {
        public void Configure(EntityTypeBuilder<VehicleLot> builder) {

            builder.ToTable("vehicle_lot");

            builder.HasKey(t => t.Id);
            builder.Property(t => t.Id).HasMaxLength(EntityFieldLen.Id).ValueGeneratedOnAdd();

            builder.Property(t => t.LotNo).HasMaxLength(EntityFieldLen.Vehicle_LotNo);
            builder.HasIndex(t => t.LotNo).IsUnique();

            // relationships
            builder.HasMany(t => t.Vehicles)
                .WithOne(t => t.Lot)
                .HasForeignKey(t => t.LotId);

            builder.HasMany(t => t.LotParts)
                .WithOne(t => t.Lot)
                .HasForeignKey(t => t.LotId);

            builder.HasOne(t => t.Plant)
                .WithMany(t => t.VehicleLots)
                .HasForeignKey(t => t.PlantId);

            builder.HasOne(t => t.Bom)
                .WithMany(t => t.Lots)
                .HasForeignKey(t => t.BomId);

        }
    }
}