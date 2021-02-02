using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SKD.Model {
    public class KitTimelineEventType_Config : IEntityTypeConfiguration<KitTimelineEventType> {
        public void Configure(EntityTypeBuilder<KitTimelineEventType> builder) {

            builder.ToTable("kit_timeline_event_type");
                
            builder.HasKey(t => t.Id);
            builder.Property(t => t.Id).HasMaxLength(EntityFieldLen.Id).ValueGeneratedOnAdd();

            builder.HasIndex(t => t.Code).IsUnique();

            builder.Property(t => t.Code).IsRequired().HasMaxLength(EntityFieldLen.Event_Code);
            builder.Property(t => t.Description).IsRequired().HasMaxLength(EntityFieldLen.Event_Description);
            
            builder.HasIndex(t => t.Code).IsUnique();            
        }

    }
}