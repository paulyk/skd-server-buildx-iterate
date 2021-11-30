namespace SKD.Model;

public class User_Config : IEntityTypeConfiguration<User> {
    public void Configure(EntityTypeBuilder<User> builder) {

        builder.ToTable("user");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasMaxLength(EntityFieldLen.Id).ValueGeneratedOnAdd();

        builder.HasIndex(t => t.Email).IsUnique();

        builder.Property(t => t.Id).HasMaxLength(EntityFieldLen.Id).ValueGeneratedOnAdd();
        builder.Property(t => t.Email)
                .IsRequired()
                .HasMaxLength(EntityFieldLen.Email);
    }
}
