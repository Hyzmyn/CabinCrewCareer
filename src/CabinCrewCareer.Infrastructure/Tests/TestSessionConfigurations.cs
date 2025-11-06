using CabinCrewCareer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CabinCrewCareer.Infrastructure.Tests
{
    public class TestSessionConfigurations : IEntityTypeConfiguration<TestSession>
    {
        public void Configure(EntityTypeBuilder<TestSession> builder)
        {
            builder.ToTable("TestSession");

            builder.HasKey(n => n.TestSessionId);

            builder.HasOne(u => u.User)
                .WithMany(u => u.TestSessions)
                .HasForeignKey(u => u.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(u => u.Test)
                .WithMany(u => u.TestSessions)
                .HasForeignKey(u => u.TestId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(u => u.EnquiryRequest)
                .WithOne(u => u.TestSession)
                .HasForeignKey<EnquiryRequest>(u => u.TestSessionId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
