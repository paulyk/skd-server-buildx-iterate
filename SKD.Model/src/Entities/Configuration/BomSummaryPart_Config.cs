using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SKD.Model {
    public class BomSummaryPart_Config : IEntityTypeConfiguration<BomSummaryPart> {
        public void Configure(EntityTypeBuilder<BomSummaryPart> builder) {
            builder.ToTable("bom_summary_part");

            builder.HasKey(t => t.Id);
            builder.Property(t => t.Id).HasMaxLength(EntityFieldLen.Id).ValueGeneratedOnAdd();

            builder.HasIndex(t => new { t.LotNo, t.PartNo }).IsUnique();
            builder.HasIndex(t => t.PartNo);

            builder.Property(t => t.LotNo).IsRequired().HasMaxLength(EntityFieldLen.BomPart_LotNo);
            builder.Property(t => t.PartNo).IsRequired().HasMaxLength(EntityFieldLen.BomPart_PartNo);
            builder.Property(t => t.PartDesc).IsRequired().HasMaxLength(EntityFieldLen.BomPart_PartDesc);
            builder.Property(t => t.Quantity).IsRequired();

            builder.HasOne(t => t.BomSummary)
                .WithMany(t => t.Parts)
                .HasForeignKey(t => t.BomSummaryId);

        }
    }
}