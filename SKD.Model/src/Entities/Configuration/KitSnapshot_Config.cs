using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SKD.Model {
    public class KitSnapshot_Config : IEntityTypeConfiguration<KitSnapshot> {
        public void Configure(EntityTypeBuilder<KitSnapshot> builder) {

            builder.ToTable("vehicle_snapshot");

            builder.HasKey(t => t.Id);
            builder.Property(t => t.Id).HasMaxLength(EntityFieldLen.Id).ValueGeneratedOnAdd();

            builder.HasIndex(t => new { t.VehicleSnapshotRunId, t.KitId }).IsUnique();

            builder.Property(t => t.VIN).HasMaxLength(EntityFieldLen.Vehicle_VIN);
            // relationships
            builder.HasOne(t => t.Kit)
                .WithMany(t => t.Snapshots)
                .OnDelete(DeleteBehavior.NoAction)
                .HasForeignKey(t => t.KitId);

        }
    }
}