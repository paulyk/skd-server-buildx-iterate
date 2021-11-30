namespace SKD.Model;

public class ShipmentInvoice_Config : IEntityTypeConfiguration<ShipmentInvoice> {
    public void Configure(EntityTypeBuilder<ShipmentInvoice> builder) {

        builder.ToTable("shipment_invoice");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasMaxLength(EntityFieldLen.Id).ValueGeneratedOnAdd();

        builder.HasIndex(t => t.InvoiceNo).IsUnique();

        builder.Property(t => t.InvoiceNo)
            .IsRequired()
            .HasMaxLength(EntityFieldLen.Shipment_InvoiceNo);

        builder.HasMany(t => t.HandlingUnits)
            .WithOne(t => t.ShipmentInvoice)
            .HasForeignKey(t => t.ShipmentInvoiceId);
    }
}
