namespace SKD.Model;

public class Shipment_Config : IEntityTypeConfiguration<Shipment> {
    public void Configure(EntityTypeBuilder<Shipment> builder) {

        builder.ToTable("shipment");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasMaxLength(EntityFieldLen.Id).ValueGeneratedOnAdd();

        builder.HasIndex(t => new { t.PlantId, t.Sequence }).IsUnique();

        builder.HasMany(t => t.ShipmentLots)
            .WithOne(t => t.Shipment)
            .HasForeignKey(t => t.ShipmentId);

        builder.HasOne(t => t.Plant)
            .WithMany(t => t.Shipments)
            .HasForeignKey(t => t.PlantId);
    }
}
