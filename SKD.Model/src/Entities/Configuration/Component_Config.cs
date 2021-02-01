using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SKD.Model {
    public class Component_Config : IEntityTypeConfiguration<Component> {
        public void Configure(EntityTypeBuilder<Component> builder) {

            builder.ToTable("component");
                
            builder.HasKey(t => t.Id);
            builder.Property(t => t.Id).HasMaxLength(EntityFieldLen.Id).ValueGeneratedOnAdd();

            builder.HasIndex(t => t.Code).IsUnique();
            builder.HasIndex(t => t.Name).IsUnique();
                            
            builder.Property(t => t.Code)
                .IsRequired()
                .HasMaxLength(EntityFieldLen.Component_Code);

            builder.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(EntityFieldLen.Component_Name);

            builder.HasMany(t => t.VehicleModelComponents)
                .WithOne(t => t.Component)
                .HasForeignKey(t => t.ComponentId);

            builder.HasMany(t => t.KitComponents)
                .WithOne(t => t.Component)
                .HasForeignKey(t => t.ComponentId);
                         
        }
    }
}