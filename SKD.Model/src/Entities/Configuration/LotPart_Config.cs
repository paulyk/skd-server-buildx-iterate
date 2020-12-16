using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SKD.Model {
    public class LotPart_Config : IEntityTypeConfiguration<LotPart> {
        public void Configure(EntityTypeBuilder<LotPart> builder) {
            builder.ToTable("lot_part");

            builder.HasKey(t => t.Id);
            builder.Property(t => t.Id).HasMaxLength(EntityFieldLen.Id).ValueGeneratedOnAdd();

            builder.HasIndex(t => new { t.LotId, t.PartNo }).IsUnique();
            builder.HasIndex(t => t.PartNo);

            builder.Property(t => t.PartNo).IsRequired().HasMaxLength(EntityFieldLen.BomPart_PartNo);
            builder.Property(t => t.PartDesc).IsRequired().HasMaxLength(EntityFieldLen.BomPart_PartDesc);
            builder.Property(t => t.Quantity).IsRequired();

            builder.HasOne(t => t.Lot)
                .WithMany(t => t.LotParts)
                .HasForeignKey(t => t.LotId);

        }
    }
}