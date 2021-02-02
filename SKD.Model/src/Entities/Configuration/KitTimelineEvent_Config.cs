using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SKD.Model {
    public class KitTimelineEvent_Config : IEntityTypeConfiguration<KitTimelineEvent> {
        public void Configure(EntityTypeBuilder<KitTimelineEvent> builder) {

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