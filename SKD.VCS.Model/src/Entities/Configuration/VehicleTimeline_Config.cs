using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SKD.VCS.Model {
    public class VehicleTimeline_Config : IEntityTypeConfiguration<VehicleTimeline> {
        public void Configure(EntityTypeBuilder<VehicleTimeline> builder) {

            builder.ToTable("vehicle_timeline");

            builder.HasKey(t => t.Id);
            builder.Property(t => t.Id).HasMaxLength(EntityFieldLen.Id).ValueGeneratedOnAdd();

        }
    }
}

