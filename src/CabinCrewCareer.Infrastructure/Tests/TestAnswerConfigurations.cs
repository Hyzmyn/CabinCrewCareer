using CabinCrewCareer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CabinCrewCareer.Infrastructure.Tests
{
    public class TestAnswerConfigurations : IEntityTypeConfiguration<TestAnswer>
    {
        public void Configure(EntityTypeBuilder<TestAnswer> builder)
        {
            builder.ToTable("TestAnswer");

            builder.HasKey(n => n.AnswerId);

            builder.Property(n => n.EssayAnswer)
                .HasMaxLength(100)
                .IsRequired();

            builder.HasOne(u => u.TestSession)
                .WithMany(u => u.TestAnswers)
                .HasForeignKey(u => u.TestSessionId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
