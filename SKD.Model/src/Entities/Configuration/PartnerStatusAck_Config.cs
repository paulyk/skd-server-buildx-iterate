namespace SKD.Model;

public class KitSnapshotRunAck_Config : IEntityTypeConfiguration<PartnerStatusAck> {
    public void Configure(EntityTypeBuilder<PartnerStatusAck> builder) {

        builder.ToTable("partner_status_ack");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasMaxLength(EntityFieldLen.Id).ValueGeneratedOnAdd();        
    }
}