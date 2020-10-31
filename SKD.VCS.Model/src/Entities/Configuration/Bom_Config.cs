using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SKD.VCS.Model {
    public class Bom_Config : IEntityTypeConfiguration<Bom> {
        public void Configure(EntityTypeBuilder<Bom> builder) {

            builder.ToTable("bom");
            
            builder.HasKey(t => t.Id);
            builder.Property(t => t.Id).HasMaxLength(EntityFieldLen.Id).ValueGeneratedOnAdd();

            builder.HasIndex(t => t.SequenceNo);
                            
            builder.Property(t => t.SequenceNo)
                .IsRequired()
                .HasMaxLength(EntityFieldLen.Bom_SequenceNo);

            builder.HasMany(t => t.Lots)
                .WithOne(t => t.Bom)
                .HasForeignKey(t => t.BomId);     

            builder.HasOne(t => t.ProductionPlant)       
                .WithMany(t => t.Boms)
                .HasForeignKey(t => t.ProductionPlantId);

        }
    }

}