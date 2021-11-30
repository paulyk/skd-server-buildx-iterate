namespace SKD.Model;

public class ComponentSerial_Config : IEntityTypeConfiguration<ComponentSerial> {
    public void Configure(EntityTypeBuilder<ComponentSerial> builder) {
        builder.ToTable("component_serial");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasMaxLength(EntityFieldLen.Id).ValueGeneratedOnAdd();

        builder.HasIndex(t => new { t.Serial1, t.Serial2 });
        builder.HasIndex(t => t.Serial2);

        builder.Property(t => t.Serial1).HasMaxLength(EntityFieldLen.ComponentSerial);
        builder.Property(t => t.Serial2).HasMaxLength(EntityFieldLen.ComponentSerial);

        builder.Property(t => t.Original_Serial1).HasMaxLength(EntityFieldLen.ComponentSerial);
        builder.Property(t => t.Original_Serial2).HasMaxLength(EntityFieldLen.ComponentSerial);

        builder.HasOne(t => t.KitComponent)
            .WithMany(t => t.ComponentSerials)
            .HasForeignKey(t => t.KitComponentId);

        builder.HasMany(t => t.DcwsResponses)
            .WithOne(t => t.ComponentSerial)
            .HasForeignKey(t => t.ComponentSerialId);
    }
}
