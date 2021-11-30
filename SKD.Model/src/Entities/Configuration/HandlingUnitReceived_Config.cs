namespace SKD.Model;

public class HandlingUnitReceived_Config : IEntityTypeConfiguration<HandlingUnitReceived> {
    public void Configure(EntityTypeBuilder<HandlingUnitReceived> builder) {

        builder.ToTable("handling_unit_received");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasMaxLength(EntityFieldLen.Id).ValueGeneratedOnAdd();

        builder.HasOne(t => t.HandlingUnit)
            .WithMany(t => t.Received)
            .HasForeignKey(t => t.HandlingUnitId);
    }
}
