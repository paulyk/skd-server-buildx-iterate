using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SKD.VCS.Model {
    public class BomSummary_Config : IEntityTypeConfiguration<BomSummary> {
        public void Configure(EntityTypeBuilder<BomSummary> builder) {

            builder.ToTable("bom_summary");
            
            builder.HasKey(t => t.Id);
            builder.Property(t => t.Id).HasMaxLength(EntityFieldLen.Id).ValueGeneratedOnAdd();

            builder.HasIndex(t => t.SequenceNo);
                            
            builder.Property(t => t.SequenceNo)
                .IsRequired()
                .HasMaxLength(EntityFieldLen.Bom_SequenceNo);

            builder.HasMany(t => t.Parts)
                .WithOne(t => t.BomSummary)
                .HasForeignKey(t => t.BomSummaryId);     

            builder.HasOne(t => t.ProductionPlant)       
                .WithMany(t => t.BomSummaries)
                .HasForeignKey(t => t.ProductionPlantId);

        }
    }

}