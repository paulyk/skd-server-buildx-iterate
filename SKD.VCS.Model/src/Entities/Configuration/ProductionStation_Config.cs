using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SKD.VCS.Model {

    public class ProductionStation_Config : IEntityTypeConfiguration<ProductionStation> {
        public void Configure(EntityTypeBuilder<ProductionStation> builder) { builder.ToTable("user");
                
            builder.ToTable("production_station");
            builder.HasKey(t => t.Id);
            builder.Property(t => t.Id).HasMaxLength(EntityMaxLen.Id).ValueGeneratedOnAdd();

            builder.HasIndex(t => t.Code).IsUnique();
            builder.HasIndex(t => t.Name).IsUnique();

            builder.Property(t => t.Code).IsRequired().HasMaxLength(EntityMaxLen.ProductionStation_Code);
            builder.Property(t => t.Code).IsRequired().HasMaxLength(EntityMaxLen.ProductionStation_Name);

            builder.HasMany(t => t.ModelComponents)
                .WithOne(t => t.ProductionStation)
                .HasForeignKey(t => t.ProductionStationId);

            builder.HasMany(t => t.VehicleComponents)
                .WithOne(t => t.ProductionStation)
                .HasForeignKey(t => t.ProductionStationId);
        }
    }
}