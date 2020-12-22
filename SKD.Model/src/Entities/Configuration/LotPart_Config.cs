using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SKD.Model {
    public class LotPart_Config : IEntityTypeConfiguration<LotPart> {
        public void Configure(EntityTypeBuilder<LotPart> builder) {
            builder.ToTable("lot_part");

            builder.HasKey(t => t.Id);
            builder.Property(t => t.Id).HasMaxLength(EntityFieldLen.Id).ValueGeneratedOnAdd();

            builder.HasIndex(t => new { t.LotId, t.PartId }).IsUnique();

            builder.Property(t => t.Quantity).IsRequired();

            builder.HasOne(t => t.Lot)
                .WithMany(t => t.LotParts)
                .HasForeignKey(t => t.LotId);

            builder.HasOne(t => t.Part)
                .WithMany(t => t.LotParts)
                .HasForeignKey(t => t.PartId);

        }
    }
}