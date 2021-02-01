using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SKD.Model {
    public class KitSnapshotRun_Config : IEntityTypeConfiguration<kitSnapshotRun> {
        public void Configure(EntityTypeBuilder<kitSnapshotRun> builder) {

            builder.ToTable("vehicle_snapshot_run");

            builder.HasKey(t => t.Id);
            builder.Property(t => t.Id).HasMaxLength(EntityFieldLen.Id).ValueGeneratedOnAdd();

            builder.HasIndex(t => new { t.PlantId, t.RunDate  } ).IsUnique();
            builder.HasIndex(t => new { t.PlantId, t.Sequence  } ).IsUnique();

            builder.HasMany(t => t.KitSnapshots)
                .WithOne(t => t.VehicleSnapshotRun)
                .HasForeignKey(t => t.VehicleSnapshotRunId);

        }
    }
}