namespace SKD.Model;

public class Bom_Config : IEntityTypeConfiguration<Bom> {
    public void Configure(EntityTypeBuilder<Bom> builder) {

        builder.ToTable("bom");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasMaxLength(EntityFieldLen.Id).ValueGeneratedOnAdd();

        builder.HasIndex(t => new { t.PlantId, t.Sequence }).IsUnique();

        builder.Property(t => t.Sequence)
            .IsRequired()
            .HasMaxLength(EntityFieldLen.Bom_SequenceNo);

        builder.HasOne(t => t.Plant)
            .WithMany(t => t.Boms)
            .HasForeignKey(t => t.PlantId);

        builder.HasMany(t => t.Lots)
            .WithOne(t => t.Bom)
            .HasForeignKey(t => t.BomId);
    }
}
