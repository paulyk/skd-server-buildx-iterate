namespace SKD.Model;

public class VehicleModelComponent_Config : IEntityTypeConfiguration<PcvComponent> {
    public void Configure(EntityTypeBuilder<PcvComponent> builder) {

        builder.ToTable("pcv_component");

        builder.HasKey(t => t.Id);
        builder.HasIndex(t => new { t.PcvId, t.ComponentId, t.ProductionStationId }).IsUnique();

        builder.Property(t => t.Id).HasMaxLength(EntityFieldLen.Id).ValueGeneratedOnAdd();

        builder.Property(t => t.PcvId).HasColumnName("PcvId");

        builder.HasOne(t => t.Pcv)
            .WithMany(t => t.PcvComponents)
            .HasForeignKey(t => t.PcvId);

        builder.HasOne(t => t.Component)
            .WithMany(t => t.VehicleModelComponents)
            .HasForeignKey(t => t.ComponentId);
    }
}
