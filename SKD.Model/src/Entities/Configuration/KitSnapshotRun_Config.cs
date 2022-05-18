namespace SKD.Model;

public class KitSnapshotRun_Config : IEntityTypeConfiguration<KitSnapshotRun> {
    public void Configure(EntityTypeBuilder<KitSnapshotRun> builder) {

        builder.ToTable("kit_snapshot_run");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasMaxLength(EntityFieldLen.Id).ValueGeneratedOnAdd();

        builder.HasIndex(t => new { t.PlantId, t.RunDate }).IsUnique();
        builder.HasIndex(t => new { t.PlantId, t.Sequence }).IsUnique();

        builder.HasMany(t => t.KitSnapshots)
            .WithOne(t => t.KitSnapshotRun)
            .HasForeignKey(t => t.KitSnapshotRunId);

        builder.HasOne(t => t.PartnerStatusAck)
            .WithOne(t => t.KitSnapshotRun)
            .HasForeignKey<PartnerStatusAck>(t => t.KitSnapshotRunId);
    }
}
