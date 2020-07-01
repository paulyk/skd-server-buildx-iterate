using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SKD.Model {
    public class VehicleModel_Config : IEntityTypeConfiguration<VehicleModel> {
        public void Configure(EntityTypeBuilder<VehicleModel> builder) {

            builder.ToTable("vehicle_model");

            builder.HasKey(t => t.Id);
            builder.Property(t => t.Id).HasMaxLength(EntityMaxLen.Id).ValueGeneratedOnAdd();

            builder.HasIndex(t => t.Code).IsUnique();
            builder.HasIndex(t => t.Name).IsUnique();

            builder.Ignore(t => t.ActiveComponentMappings);

            builder.Property(t => t.Code)
                .IsRequired()
                .HasMaxLength(EntityMaxLen.VehicleModel_Code);

            builder.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(EntityMaxLen.VehicleModel_Name);

            builder.HasMany(t => t.Vehicles)
                .WithOne(t => t.Model)
                .HasForeignKey(t => t.ModelId);
        }
    }
}