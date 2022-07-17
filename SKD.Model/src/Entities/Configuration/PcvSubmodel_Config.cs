namespace SKD.Model;

public class PcvSubmodel_Config : IEntityTypeConfiguration<PcvSubmodel> {
    public void Configure(EntityTypeBuilder<PcvSubmodel> builder) {

        builder.ToTable("pcv_submodel");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasMaxLength(EntityFieldLen.Id).ValueGeneratedOnAdd();
        builder.Property(t => t.Code).HasMaxLength(EntityFieldLen.RefData_Code);
        builder.Property(t => t.Name).HasMaxLength(EntityFieldLen.RefData_Name);

        builder.HasIndex(t => t.Code).IsUnique();
        builder.HasIndex(t => t.Name).IsUnique();

        builder.HasMany(t => t.SubmodelComponents)
            .WithOne(t => t.Submodel)
            .HasForeignKey(t => t.SubmodelId);

        builder.HasMany(t => t.Pcvs)
            .WithOne(t => t.SubModel)
            .HasForeignKey(t => t.SubModelId);

    }
}