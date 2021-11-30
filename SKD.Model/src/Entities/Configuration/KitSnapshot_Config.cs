namespace SKD.Model;

public class KitSnapshot_Config : IEntityTypeConfiguration<KitSnapshot> {
    public void Configure(EntityTypeBuilder<KitSnapshot> builder) {

        builder.ToTable("kit_snapshot");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasMaxLength(EntityFieldLen.Id).ValueGeneratedOnAdd();

        builder.HasIndex(t => new { t.KitSnapshotRunId, t.KitId }).IsUnique();

        builder.Property(t => t.VIN).HasMaxLength(EntityFieldLen.VIN);

        // relationships
        builder.HasOne(t => t.Kit)
            .WithMany(t => t.Snapshots)
            .OnDelete(DeleteBehavior.NoAction)
            .HasForeignKey(t => t.KitId);

        builder.HasOne(t => t.KitTimeLineEventType)
            .WithMany(t => t.Snapshots)
            .HasForeignKey(t => t.KitTimeLineEventTypeId);

    }
}
