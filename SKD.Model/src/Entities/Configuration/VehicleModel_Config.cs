using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SKD.Model {
    public class VehicleModel_Config : IEntityTypeConfiguration<VehicleModel> {
        public void Configure(EntityTypeBuilder<VehicleModel> builder) {

            builder.ToTable("vehicle_model");

            builder.HasKey(t => t.Id);
            builder.Property(t => t.Id).HasMaxLength(EntityFieldLen.Id).ValueGeneratedOnAdd();

            builder.HasIndex(t => t.Code).IsUnique();
            builder.HasIndex(t => t.Name).IsUnique();

            builder.Ignore(t => t.ActiveComponentMappings);

            // relationships            
            builder.Property(t => t.Code)
                .IsRequired()
                .HasMaxLength(EntityFieldLen.VehicleModel_Code);

            builder.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(EntityFieldLen.VehicleModel_Name);

            builder.HasMany(t => t.Lots)
                .WithOne(t => t.Model)
                .HasForeignKey(t => t.ModelId);

            builder.HasMany(t => t.Vehicles)
                .WithOne(t => t.Model)
                .HasForeignKey(t => t.ModelId);
        }
    }
}