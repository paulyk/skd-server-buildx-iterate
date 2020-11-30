using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SKD.Model {
    public class VehicleSnapshotRun_Config : IEntityTypeConfiguration<VehicleSnapshotRun> {
        public void Configure(EntityTypeBuilder<VehicleSnapshotRun> builder) {

            builder.ToTable("vehicle_snapshot_run");

            builder.HasKey(t => t.Id);
            builder.Property(t => t.Id).HasMaxLength(EntityFieldLen.Id).ValueGeneratedOnAdd();

            builder.HasIndex(t => new { t.PlantId, t.RunDate  } ).IsUnique();
            builder.HasIndex(t => new { t.PlantId, t.Sequence  } ).IsUnique();

            builder.HasMany(t => t.VehicleSnapshots)
                .WithOne(t => t.VehicleSnapshotRun)
                .HasForeignKey(t => t.VehicleSnapshotRunId);

        }
    }
}