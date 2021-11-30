
namespace SKD.Model;

public class Dealer_Config : IEntityTypeConfiguration<Dealer> {
    public void Configure(EntityTypeBuilder<Dealer> builder) {

        builder.ToTable("dealer");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasMaxLength(EntityFieldLen.Id).ValueGeneratedOnAdd();

        builder.HasIndex(t => t.Code).IsUnique();
        builder.HasIndex(t => t.Name).IsUnique();

        builder.HasMany(t => t.Kits)
            .WithOne(t => t.Dealer)
            .HasForeignKey(t => t.DealerId);
    }
}
