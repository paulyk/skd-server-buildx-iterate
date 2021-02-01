using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SKD.Model {
    public class Kit_Config : IEntityTypeConfiguration<Kit> {
        public void Configure(EntityTypeBuilder<Kit> builder) {

            builder.ToTable("vehicle");
                
            builder.HasKey(t => t.Id);
            builder.Property(t => t.Id).HasMaxLength(EntityFieldLen.Id).ValueGeneratedOnAdd();

            builder.HasIndex(t => t.KitNo).IsUnique();
            builder.HasIndex(t => t.VIN);

            builder.Property(t => t.KitNo).IsRequired().HasMaxLength(EntityFieldLen.Vehicle_KitNo);
            builder.Property(t => t.VIN).HasMaxLength(EntityFieldLen.Vehicle_VIN);

            // relationships
            builder.HasOne(t => t.Model)
                .WithMany(t => t.Vehicles)
                .HasForeignKey(t => t.ModelId);

            builder.HasOne(t => t.Lot)
                .WithMany(t => t.Kits)
                .HasForeignKey(t => t.LotId);

            builder.HasMany(t => t.KitComponents)
                .WithOne(t => t.Kit)
                .HasForeignKey(t => t.KitId);

            builder.HasMany(t => t.TimelineEvents)
                .WithOne(t => t.Vehicle)
                .HasForeignKey(t => t.VehicleId);

            builder.HasMany(t => t.Snapshots)
                .WithOne(t => t.Kit)
                .HasForeignKey(t => t.KitId);
        }
    }
}