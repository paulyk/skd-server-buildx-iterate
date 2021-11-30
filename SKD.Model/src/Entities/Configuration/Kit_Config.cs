namespace SKD.Model;

public class Kit_Config : IEntityTypeConfiguration<Kit> {
    public void Configure(EntityTypeBuilder<Kit> builder) {

        builder.ToTable("kit");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasMaxLength(EntityFieldLen.Id).ValueGeneratedOnAdd();

        builder.HasIndex(t => t.KitNo).IsUnique();
        builder.HasIndex(t => t.VIN);

        builder.Property(t => t.KitNo).IsRequired().HasMaxLength(EntityFieldLen.KitNo);
        builder.Property(t => t.VIN).HasMaxLength(EntityFieldLen.VIN);

        builder.HasOne(t => t.Lot)
            .WithMany(t => t.Kits)
            .HasForeignKey(t => t.LotId);

        builder.HasMany(t => t.KitComponents)
            .WithOne(t => t.Kit)
            .HasForeignKey(t => t.KitId);

        builder.HasMany(t => t.TimelineEvents)
            .WithOne(t => t.Kit)
            .HasForeignKey(t => t.KitId);

        builder.HasMany(t => t.Snapshots)
            .WithOne(t => t.Kit)
            .HasForeignKey(t => t.KitId);

        builder.HasOne(t => t.Dealer)
            .WithMany(t => t.Kits)
            .HasForeignKey(t => t.DealerId);
    }
}
