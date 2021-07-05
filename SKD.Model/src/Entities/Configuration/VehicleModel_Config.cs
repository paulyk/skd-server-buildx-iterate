using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SKD.Model {
    public class VehicleModel_Config : IEntityTypeConfiguration<VehicleModel> {
        public void Configure(EntityTypeBuilder<VehicleModel> builder) {

            builder.ToTable("vehicle_model");

            builder.HasKey(t => t.Id);
            builder.Property(t => t.Id).HasMaxLength(EntityFieldLen.Id).ValueGeneratedOnAdd();
            builder.Property(t => t.Code).IsRequired().HasMaxLength(EntityFieldLen.VehicleModel_Code).ValueGeneratedOnAdd();
            builder.Property(t => t.Name).IsRequired().HasMaxLength(EntityFieldLen.VehicleModel_Description).ValueGeneratedOnAdd();
            builder.Property(t => t.Model).HasMaxLength(EntityFieldLen.VehicleModel_Model).ValueGeneratedOnAdd();
            builder.Property(t => t.ModelYear).HasMaxLength(EntityFieldLen.VehicleModel_ModelYear).ValueGeneratedOnAdd();
            builder.Property(t => t.Series).HasMaxLength(EntityFieldLen.VehicleModel_Series).ValueGeneratedOnAdd();
            builder.Property(t => t.Body).HasMaxLength(EntityFieldLen.VehicleModel_Series).ValueGeneratedOnAdd();

            // index
            builder.HasIndex(t => t.Code).IsUnique();

            // relationships            
            builder.HasMany(t => t.Lots)
                .WithOne(t => t.Model)
                .HasForeignKey(t => t.ModelId);
        }
    }
}