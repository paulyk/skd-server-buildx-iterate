namespace SKD.Model;

public class PcvSubmodelComponent_Config : IEntityTypeConfiguration<PcvSubmodelComponent> {
    public void Configure(EntityTypeBuilder<PcvSubmodelComponent> builder) {

        builder.ToTable("pcv_submodel_component");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasMaxLength(EntityFieldLen.Id).ValueGeneratedOnAdd();

        builder.HasOne(t => t.Submodel)
            .WithMany(t => t.SubmodelComponents)
            .HasForeignKey(t => t.SubmodelId);

        builder.HasOne(t => t.Component)
            .WithMany(t => t.SubmodelComponents)
            .HasForeignKey(t => t.ComponentId);
    }
}