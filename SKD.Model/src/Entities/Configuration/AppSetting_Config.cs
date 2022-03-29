namespace SKD.Model;

public class AppSetting_Config : IEntityTypeConfiguration<AppSetting> {
    public void Configure(EntityTypeBuilder<AppSetting> builder) {

        builder.ToTable("app_setting");
        
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasMaxLength(EntityFieldLen.Id).ValueGeneratedOnAdd();

        builder.HasIndex(t => t.Code).IsUnique();
    }
}