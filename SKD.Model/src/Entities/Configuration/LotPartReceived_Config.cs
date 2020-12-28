using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SKD.Model {
    public class LotPartReceived_Config : IEntityTypeConfiguration<LotPartReceived> {
        public void Configure(EntityTypeBuilder<LotPartReceived> builder) {

            builder.ToTable("lot_part_received");
                
            builder.HasKey(t => t.Id);
            builder.Property(t => t.Id).HasMaxLength(EntityFieldLen.Id).ValueGeneratedOnAdd();

            builder.HasIndex(t => t.LotPartId).IsUnique();

            // relationships     
            builder.HasOne(t => t.LotPart)
                .WithMany(t => t.Received)
                .HasForeignKey(t => t.LotPartId);

        }
    }
}