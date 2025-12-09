using CabinCrewCareer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CabinCrewCareer.Infrastructure.Tests
{
    public class TestConfigurations : IEntityTypeConfiguration<Test>
    {
        public void Configure(EntityTypeBuilder<Test> builder)
        {
            builder.ToTable("Test");

            builder.HasKey(n => n.TestId);

            builder.Property(n => n.TestName)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(n => n.Purpose)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(n => n.AudioFileURL)
                .IsRequired();

            builder.Property(n => n.JoinCode)
                .HasMaxLength(20)
                .IsRequired();

            builder.HasOne(u => u.Type)
                .WithMany(u => u.Tests)
                .HasForeignKey(u => u.TestTypeId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
