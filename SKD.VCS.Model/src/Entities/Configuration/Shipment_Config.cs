using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SKD.VCS.Model {
    public class Shipment_Config : IEntityTypeConfiguration<Shipment> {
        public void Configure(EntityTypeBuilder<Shipment> builder) {

            builder.ToTable("shipment");
                
            builder.HasKey(t => t.Id);
            builder.Property(t => t.Id).HasMaxLength(EntityFieldLen.Id).ValueGeneratedOnAdd();

            builder.HasIndex(t => t.ShipSequenceNo); // not unique

            builder.Property(t => t.ShipSequenceNo)
                .IsRequired()
                .HasMaxLength(EntityFieldLen.Shipment_SequenceNo);

            builder.HasMany(t => t.Lots)
                .WithOne(t => t.Shipment)
                .HasForeignKey(t => t.ShipmentId);

        }
    }
}