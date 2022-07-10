namespace SKD.Model;

public class Pcv_Config : IEntityTypeConfiguration<PCV> {
    public void Configure(EntityTypeBuilder<PCV> builder) {

        builder.ToTable("pcv");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasMaxLength(EntityFieldLen.Id).ValueGeneratedOnAdd();
        builder.Property(t => t.Code).IsRequired().HasMaxLength(EntityFieldLen.Pcv_Code).ValueGeneratedOnAdd();
        builder.Property(t => t.Description).IsRequired().HasMaxLength(EntityFieldLen.Pcv_Description).ValueGeneratedOnAdd();
        builder.Property(t => t.Model).HasMaxLength(EntityFieldLen.Pcv_Meta).ValueGeneratedOnAdd();
        builder.Property(t => t.ModelYear).HasMaxLength(EntityFieldLen.Pcv_Meta).ValueGeneratedOnAdd();
        builder.Property(t => t.Series).HasMaxLength(EntityFieldLen.Pcv_Meta).ValueGeneratedOnAdd();
        builder.Property(t => t.Body).HasMaxLength(EntityFieldLen.Pcv_Meta).ValueGeneratedOnAdd();

        // index
        builder.HasIndex(t => t.Code).IsUnique();

        // relationships            
        builder.HasMany(t => t.Lots)
            .WithOne(t => t.Pcv)
            .HasForeignKey(t => t.ModelId);
    }
}
