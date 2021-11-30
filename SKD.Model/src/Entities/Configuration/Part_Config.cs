namespace SKD.Model;

public class Part_Config : IEntityTypeConfiguration<Part> {
    public void Configure(EntityTypeBuilder<Part> builder) {

        builder.ToTable("part");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasMaxLength(EntityFieldLen.Id).ValueGeneratedOnAdd();

        builder.HasIndex(t => t.PartNo).IsUnique();
        builder.HasIndex(t => t.PartDesc);

        builder.Property(t => t.PartNo).IsRequired().HasMaxLength(EntityFieldLen.Part_No);
        builder.Property(t => t.OriginalPartNo).IsRequired().HasMaxLength(EntityFieldLen.Part_No);
        builder.Property(t => t.PartDesc).HasMaxLength(EntityFieldLen.Part_Desc);

        // relationships     
        builder.HasMany(t => t.LotParts)
            .WithOne(t => t.Part)
            .HasForeignKey(t => t.PartId);

    }
}
