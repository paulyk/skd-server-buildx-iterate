using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SKD.VCS.Model {
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
        }
    }
}