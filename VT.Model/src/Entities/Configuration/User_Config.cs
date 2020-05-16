
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace VT.Model {
    public class User_Config : IEntityTypeConfiguration<User> {
        public void Configure(EntityTypeBuilder<User> builder) {
            
            builder.ToTable("user");
                
            builder.HasKey(t => t.Id);
            builder.HasIndex(t => t.Email).IsUnique();

            builder.Property(t => t.Id).HasMaxLength(EntityMaxLen.Id).ValueGeneratedOnAdd();
            builder.Property(t => t.Email)
                    .IsRequired()
                    .HasMaxLength(EntityMaxLen.Email);
                                             
        }
    }
}