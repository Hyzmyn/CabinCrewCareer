using CabinCrewCareer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CabinCrewCareer.Infrastructure.Tests
{
    public class EnquiryRequestConfigurations : IEntityTypeConfiguration<EnquiryRequest>
    {
        public void Configure(EntityTypeBuilder<EnquiryRequest> builder)
        {
            builder.ToTable("EnquiryRequest");

            builder.HasKey(n => n.EnquiryId);

            builder.Property(n => n.Reason)
                .HasMaxLength(250)
                .IsRequired();

            builder.HasOne(u => u.User)
                .WithMany(u => u.EnquiryRequests)
                .HasForeignKey(u => u.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            //builder.HasOne(u => u.TestResult)
            //    .WithOne(u => u.EnquiryRequest)
            //    .HasForeignKey<TestResult>(u => u.EnquiryId)
            //    .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
