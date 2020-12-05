using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SKD.Model {
    public class Shipment_Config : IEntityTypeConfiguration<Shipment> {
        public void Configure(EntityTypeBuilder<Shipment> builder) {

            builder.ToTable("shipment");
                
            builder.HasKey(t => t.Id);
            builder.Property(t => t.Id).HasMaxLength(EntityFieldLen.Id).ValueGeneratedOnAdd();

            builder.HasIndex(t => t.Sequence); // not unique

            builder.HasMany(t => t.Lots)
                .WithOne(t => t.Shipment)
                .HasForeignKey(t => t.ShipmentId);

        }
    }
}