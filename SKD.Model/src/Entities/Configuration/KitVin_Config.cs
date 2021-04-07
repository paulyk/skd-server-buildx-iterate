using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SKD.Model {
    public class KitVin_Config : IEntityTypeConfiguration<KitVin> {
        public void Configure(EntityTypeBuilder<KitVin> builder) {

            builder.ToTable("kit_vin");
            
            builder.HasKey(t => t.Id);
            builder.Property(t => t.Id).HasMaxLength(EntityFieldLen.Id).ValueGeneratedOnAdd();
                          
            builder.HasIndex(t => t.VIN).IsUnique();

            builder.Property(t => t.VIN)
                .IsRequired()
                .HasMaxLength(EntityFieldLen.VIN);

            builder.HasOne(t => t.Kit) 
                .WithMany(t => t.KitVins)
                .HasForeignKey(t => t.KitId);
                
            builder.HasOne(t => t.KitVinImport)
                .WithMany(t => t.KitVins)
                .HasForeignKey(t => t.KitVinImportId)
                .OnDelete(DeleteBehavior.Restrict);

        }
    }

}