using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SKD.Model {
    public class KitVinImport_Config : IEntityTypeConfiguration<KitVinImport> {
        public void Configure(EntityTypeBuilder<KitVinImport> builder) {

            builder.ToTable("kit_vin_import");
            
            builder.HasKey(t => t.Id);
            builder.Property(t => t.Id).HasMaxLength(EntityFieldLen.Id).ValueGeneratedOnAdd();

            builder.HasIndex(t => new { t.PlantId, t.Sequence }).IsUnique();
                            
            builder.Property(t => t.Sequence)
                .IsRequired()
                .HasMaxLength(EntityFieldLen.KitVinImport_Sequence);

            builder.Property(t => t.GSDB_Code)
                .IsRequired()
                .HasMaxLength(EntityFieldLen.GSDB_Code);

            builder.HasOne(t => t.Plant) 
                .WithMany(t => t.KitVinImports)
                .HasForeignKey(t => t.PlantId);

            builder.HasMany(t => t.KitVins)
                .WithOne(t => t.KitVinImport)
                .HasForeignKey(t => t.KitVinImportId);
        }
    }

}