using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SKD.Model {
    public class DCWSResponse_Config : IEntityTypeConfiguration<DCWSResponse> {
        public void Configure(EntityTypeBuilder<DCWSResponse> builder) {

            builder.ToTable("dcws_response");
                
            builder.HasKey(t => t.Id);
            builder.Property(t => t.Id).HasMaxLength(EntityFieldLen.Id).ValueGeneratedOnAdd();

            builder.Property(t => t.ResponseCode).HasMaxLength(EntityFieldLen.DCWSResponse_Code);            
            builder.Property(t => t.ErrorMessage).HasMaxLength(EntityFieldLen.DCWS_ErrorMessage);
                            
            builder.HasOne(t => t.ComponentScan)
                .WithMany(t => t.DCWSResponses)
                .HasForeignKey(t => t.ComponentScanId);
                         
        }
    }
}