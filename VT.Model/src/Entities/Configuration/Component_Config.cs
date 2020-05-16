
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace VT.Model {
    public class Component_Config : IEntityTypeConfiguration<Component> {
        public void Configure(EntityTypeBuilder<Component> builder) {

            builder.ToTable("component");
                
            builder.HasKey(t => t.Id);
            builder.Property(t => t.Id).HasMaxLength(EntityMaxLen.Id).ValueGeneratedOnAdd();

            builder.HasIndex(t => t.Code).IsUnique();
            builder.HasIndex(t => t.Name);
                            
            builder.Property(t => t.Code)
                .IsRequired()
                .HasMaxLength(EntityMaxLen.Component_Code);

            builder.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(EntityMaxLen.Component_Name);

            builder.Property(t => t.Type)
                .IsRequired()
                .HasMaxLength(EntityMaxLen.Component_Type);

            //
            builder.HasMany(t => t.VehicleModelComponents)
                .WithOne(t => t.Component)
                .HasForeignKey(t => t.ComponentId);

            builder.HasMany(t => t.VehicleComponents)
                .WithOne(t => t.Component)
                .HasForeignKey(t => t.ComponentId);
                         
        }
    }
}