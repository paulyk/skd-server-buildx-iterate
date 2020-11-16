using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SKD.VCS.Model {
    public class VehicleTimelineEvent_Config : IEntityTypeConfiguration<VehicleTimelineEvent> {
        public void Configure(EntityTypeBuilder<VehicleTimelineEvent> builder) {

            builder.ToTable("vehicle_timeline_event");
                
            builder.HasKey(t => t.Id);
            builder.HasIndex(t => t.CreatedAt);

            builder.Property(t => t.Id).HasMaxLength(EntityFieldLen.Id).ValueGeneratedOnAdd();
            builder.Property(t => t.EventNote).HasMaxLength(EntityFieldLen.Event_Note);

            builder.HasOne(t => t.Vehicle)
                .WithMany(t => t.TimelineEvents)
                .HasForeignKey(t => t.VehicleId);
        }

    }
}