using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SKD.VCS.Model {
    public class BomLot_Config : IEntityTypeConfiguration<BomLot> {

        public void Configure(EntityTypeBuilder<BomLot> builder) {
            builder.ToTable("bom_lot");
            
            builder.HasKey(t => t.Id);
            builder.Property(t => t.Id).HasMaxLength(EntityFieldLen.Id).ValueGeneratedOnAdd();

            builder.HasIndex(t => t.LotNo);

            builder.Property(t => t.LotNo).IsRequired().HasMaxLength(EntityFieldLen.BomLot_LotNo);

            builder.HasMany(t => t.Parts)
                .WithOne(t => t.BomLot)
                .HasForeignKey(t => t.BomLotId);
        }
    }
}