using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SKD.VCS.Model {
    public class ShipmentPart_Config : IEntityTypeConfiguration<ShipmentPart> {
        public void Configure(EntityTypeBuilder<ShipmentPart> builder) {

            builder.ToTable("shipment_part");

            builder.HasKey(t => t.Id);
            builder.Property(t => t.Id).HasMaxLength(EntityFieldLen.Id).ValueGeneratedOnAdd();

            builder.HasIndex(t => t.PartNo).IsUnique();

            builder.Property(t => t.PartNo)
                .IsRequired()
                .HasMaxLength(EntityFieldLen.Shipment_PartNo);

            builder.Property(t => t.CustomerPartNo)
                .HasMaxLength(EntityFieldLen.Shipment_CustomerPartNo);

            builder.Property(t => t.CustomerPartDesc)
                .IsRequired()
                .HasMaxLength(EntityFieldLen.Shipment_CustomerPartDesc);

            builder.HasOne(t => t.ShipmentInvoice)
                .WithMany(t => t.Parts)
                .HasForeignKey(t => t.ShipmentInvoiceId);
        }
    }
}