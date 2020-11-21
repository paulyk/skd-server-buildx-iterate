using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SKD.Model {
    public class ShipmentLot_Config : IEntityTypeConfiguration<ShipmentLot> {
        public void Configure(EntityTypeBuilder<ShipmentLot> builder) {

            builder.ToTable("shipment_lot");

            builder.HasKey(t => t.Id);
            builder.Property(t => t.Id).HasMaxLength(EntityFieldLen.Id).ValueGeneratedOnAdd();

            builder.HasIndex(t => t.LotNo);

            builder.Property(t => t.LotNo)
                .IsRequired()
                .HasMaxLength(EntityFieldLen.Shipment_LotNo);

            builder.HasMany(t => t.Invoices)
                .WithOne(t => t.ShipmentLot)
                .HasForeignKey(t => t.ShipmentLotId);
        }
    }
}