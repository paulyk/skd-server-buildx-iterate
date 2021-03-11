using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SKD.Model {
    public class HandlingUnit_Config : IEntityTypeConfiguration<HandlingUnit> {
        public void Configure(EntityTypeBuilder<HandlingUnit> builder) {

            builder.ToTable("handling_unit");
                
            builder.HasKey(t => t.Id);
            builder.Property(t => t.Id).HasMaxLength(EntityFieldLen.Id).ValueGeneratedOnAdd();

            builder.Property(t => t.Code).HasMaxLength(EntityFieldLen.HandlingUnit_Code);
            builder.HasIndex(t => t.Code).IsUnique();

            builder.HasMany(t => t.Parts)
                .WithOne(t => t.HandlingUnit)
                .HasForeignKey(t => t.HandlingUnitId);

            builder.HasOne(t => t.ShipmentInvoice)
                .WithMany(t => t.HandlingUnits)
                .HasForeignKey(t => t.ShipmentInvoiceId);
                        
            builder.HasMany(t => t.Received)
                .WithOne(t => t.HandlingUnit)
                .HasForeignKey(t => t.HandlingUnitId);
        }
    }
}