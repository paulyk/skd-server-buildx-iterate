namespace SKD.Model;

public class DCWSResponse_Config : IEntityTypeConfiguration<DcwsResponse> {
    public void Configure(EntityTypeBuilder<DcwsResponse> builder) {

        builder.ToTable("dcws_response");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasMaxLength(EntityFieldLen.Id).ValueGeneratedOnAdd();

        builder.Property(t => t.ProcessExcptionCode).HasMaxLength(EntityFieldLen.DCWSResponse_Code);
        builder.Property(t => t.ErrorMessage).HasMaxLength(EntityFieldLen.DCWS_ErrorMessage);

        builder.HasOne(t => t.ComponentSerial)
            .WithMany(t => t.DcwsResponses)
            .HasForeignKey(t => t.ComponentSerialId);

    }
}
