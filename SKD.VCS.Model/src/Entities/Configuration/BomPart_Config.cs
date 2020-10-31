using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SKD.VCS.Model {
    public class BomPart_Config : IEntityTypeConfiguration<BomPart> {
        public void Configure(EntityTypeBuilder<BomPart> builder) {
            builder.ToTable("bom_part");

            builder.HasKey(t => t.Id);
            builder.Property(t => t.Id).HasMaxLength(EntityFieldLen.Id).ValueGeneratedOnAdd();

            builder.HasIndex(t => t.PartNo);

            builder.Property(t => t.KitNo).IsRequired().HasMaxLength(EntityFieldLen.BomPart_KitNo);
            builder.Property(t => t.PartNo).IsRequired().HasMaxLength(EntityFieldLen.BomPart_PartNo);
            builder.Property(t => t.PartDesc).IsRequired().HasMaxLength(EntityFieldLen.BomPart_PartDesc);
            builder.Property(t => t.Quantity).IsRequired();

            builder.HasOne(t => t.BomLot)
                .WithMany(t => t.Parts)
                .HasForeignKey(t => t.BomLotId);

        }
    }
}