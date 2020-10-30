using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SKD.VCS.Model {
    public class ProductionPlant_Config : IEntityTypeConfiguration<ProductionPlant> {
        public void Configure(EntityTypeBuilder<ProductionPlant> builder) {

            builder.ToTable("production_plant");
                
            builder.HasKey(t => t.Id);
            builder.Property(t => t.Id).HasMaxLength(EntityFieldLen.Id).ValueGeneratedOnAdd();

            builder.HasIndex(t => t.Code).IsUnique();
            builder.HasIndex(t => t.Name).IsUnique();
                            
            builder.Property(t => t.Code)
                .IsRequired()
                .HasMaxLength(EntityFieldLen.ProductionPlant_Code);

            builder.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(EntityFieldLen.ProductionPlant_Name);

            builder.HasMany(t => t.Shipments)
                .WithOne(t => t.ProductionPlant)
                .HasForeignKey(t => t.ProductionPlantId);                         
        }
    }
}