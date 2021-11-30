namespace SKD.Model;

public class ShipmentPart_Config : IEntityTypeConfiguration<ShipmentPart> {
    public void Configure(EntityTypeBuilder<ShipmentPart> builder) {

        builder.ToTable("shipment_part");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasMaxLength(EntityFieldLen.Id).ValueGeneratedOnAdd();

        builder.HasIndex(t => new { t.HandlingUnitId, t.PartId }).IsUnique();

        builder.HasOne(t => t.Part)
            .WithMany(t => t.ShipmentParts)
            .HasForeignKey(t => t.PartId);
    }
}
