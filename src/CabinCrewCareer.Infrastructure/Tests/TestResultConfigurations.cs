using CabinCrewCareer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CabinCrewCareer.Infrastructure.Tests
{
    public class TestResultConfigurations : IEntityTypeConfiguration<TestResult>
    {
        public void Configure(EntityTypeBuilder<TestResult> builder)
        {
            builder.ToTable("TestResult");

            builder.HasKey(n => n.ResultId);

            builder.Property(n => n.EnquiryBy)
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(n => n.EnquiryNote)
                .HasMaxLength(100)
                .IsRequired();

            //builder.HasOne(u => u.Test)
            //    .WithMany(u => u.TestResults)
            //    .HasForeignKey(u => u.TestId)
            //    .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(u => u.TestSession)
                .WithMany(u => u.TestResults)
                .HasForeignKey(u => u.TestSessionId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(u => u.User)
                .WithMany(u => u.TestResults)
                .HasForeignKey(u => u.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(u => u.Examiner)
                .WithMany(u => u.TestResults)
                .HasForeignKey(u => u.ExaminerId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
