namespace SKD.Model;

public class ProductionStation_Config : IEntityTypeConfiguration<ProductionStation> {
    public void Configure(EntityTypeBuilder<ProductionStation> builder) {
        builder.ToTable("production_station");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasMaxLength(EntityFieldLen.Id).ValueGeneratedOnAdd();

        builder.HasIndex(t => t.Code).IsUnique();
        builder.HasIndex(t => t.Name).IsUnique();

        builder.Property(t => t.Code).IsRequired().HasMaxLength(EntityFieldLen.ProductionStation_Code);
        builder.Property(t => t.Code).IsRequired().HasMaxLength(EntityFieldLen.ProductionStation_Name);

        builder.HasMany(t => t.ModelComponents)
            .WithOne(t => t.ProductionStation)
            .HasForeignKey(t => t.ProductionStationId);

        builder.HasMany(t => t.KitComponents)
            .WithOne(t => t.ProductionStation)
            .HasForeignKey(t => t.ProductionStationId);

        builder.HasMany(t => t.DefaultStationComponents)
            .WithOne(t => t.ProductionStation)
            .HasForeignKey(t => t.ProductionStationId);
    }
}
