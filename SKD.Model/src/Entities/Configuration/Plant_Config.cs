using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SKD.Model {
    public class Plant_Config : IEntityTypeConfiguration<Plant> {
        public void Configure(EntityTypeBuilder<Plant> builder) {

            builder.ToTable("plant");
                
            builder.HasKey(t => t.Id);
            builder.Property(t => t.Id).HasMaxLength(EntityFieldLen.Id).ValueGeneratedOnAdd();

            builder.HasIndex(t => t.Code).IsUnique();

            builder.Property(t => t.Code).IsRequired().HasMaxLength(EntityFieldLen.Plant_Code);
            builder.Property(t => t.Name).HasMaxLength(EntityFieldLen.Plant_Name);

            // relationships        
            builder.HasMany(t => t.VehicleLots)
                .WithOne(t => t.Plant)
                .HasForeignKey(t => t.PlantId);           

            builder.HasMany(t => t.VehicleSnapshots) 
                .WithOne(t => t.Plant)
                .HasForeignKey(t => t.PlanBuild);
        }
    }
}