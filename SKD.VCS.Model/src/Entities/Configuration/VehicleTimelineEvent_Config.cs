using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SKD.VCS.Model {
    public class VehicleTimelineEvent_Config : IEntityTypeConfiguration<VehicleTimelineEvent> {
        public void Configure(EntityTypeBuilder<VehicleTimelineEvent> builder) {

            builder.ToTable("vehicle_timeline_event");
                
            builder.HasKey(t => t.Id);
            builder.Property(t => t.Id).HasMaxLength(EntityFieldLen.Id).ValueGeneratedOnAdd();

            builder.HasIndex(t => t.CreatedAt);

            builder.HasOne(t => t.Vehicle)
                .WithMany(t => t.TimelineEvents)
                .HasForeignKey(t => t.VehicleId);
        }

    }
}