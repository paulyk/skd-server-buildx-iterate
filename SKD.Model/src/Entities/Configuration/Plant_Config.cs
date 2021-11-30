namespace SKD.Model;

public class Plant_Config : IEntityTypeConfiguration<Plant> {
    public void Configure(EntityTypeBuilder<Plant> builder) {

        builder.ToTable("plant");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasMaxLength(EntityFieldLen.Id).ValueGeneratedOnAdd();

        builder.HasIndex(t => t.Code).IsUnique();
        builder.HasIndex(t => t.Name).IsUnique();

        builder.Property(t => t.Code).IsRequired().HasMaxLength(EntityFieldLen.Plant_Code);
        builder.Property(t => t.Name).HasMaxLength(EntityFieldLen.Plant_Name);

        builder.Property(t => t.PartnerPlantCode).IsRequired().HasMaxLength(EntityFieldLen.PartnerPlant_Code);
        builder.Property(t => t.PartnerPlantType).IsRequired().HasMaxLength(EntityFieldLen.PartnerPlant_Type);

        // relationships        
        builder.HasMany(t => t.Lots)
            .WithOne(t => t.Plant)
            .HasForeignKey(t => t.PlantId);

        builder.HasMany(t => t.KitSnapshotRuns)
            .WithOne(t => t.Plant)
            .HasForeignKey(t => t.PlantId);

        builder.HasMany(t => t.Boms)
            .WithOne(t => t.Plant)
            .HasForeignKey(t => t.PlantId);

        builder.HasMany(t => t.Shipments)
            .WithOne(t => t.Plant)
            .HasForeignKey(t => t.PlantId);
    }
}
