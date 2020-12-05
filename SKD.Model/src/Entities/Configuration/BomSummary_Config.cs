using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SKD.Model {
    public class BomSummary_Config : IEntityTypeConfiguration<BomSummary> {
        public void Configure(EntityTypeBuilder<BomSummary> builder) {

            builder.ToTable("bom_summary");
            
            builder.HasKey(t => t.Id);
            builder.Property(t => t.Id).HasMaxLength(EntityFieldLen.Id).ValueGeneratedOnAdd();

            builder.HasIndex(t => new { t.PlantId, t.Sequence }).IsUnique();
                            
            builder.Property(t => t.Sequence)
                .IsRequired()
                .HasMaxLength(EntityFieldLen.Bom_SequenceNo);

            builder.HasMany(t => t.Parts)
                .WithOne(t => t.BomSummary)
                .HasForeignKey(t => t.BomSummaryId);    

            builder.HasOne(t => t.Plant) 
                .WithMany(t => t.BomSummaries)
                .HasForeignKey(t => t.PlantId);

        }
    }

}