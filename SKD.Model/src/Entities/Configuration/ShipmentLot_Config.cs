namespace SKD.Model;

public class ShipmentLot_Config : IEntityTypeConfiguration<ShipmentLot> {
    public void Configure(EntityTypeBuilder<ShipmentLot> builder) {

        builder.ToTable("shipment_lot");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasMaxLength(EntityFieldLen.Id).ValueGeneratedOnAdd();

        builder.HasIndex(t => t.LotId).IsUnique();

        builder.HasMany(t => t.Invoices)
            .WithOne(t => t.ShipmentLot)
            .HasForeignKey(t => t.ShipmentLotId);

        builder.HasOne(t => t.Lot)
            .WithMany(t => t.ShipmentLots)
            .HasForeignKey(t => t.LotId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
