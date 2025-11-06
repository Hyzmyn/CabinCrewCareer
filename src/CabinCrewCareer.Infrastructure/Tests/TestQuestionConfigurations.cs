using CabinCrewCareer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CabinCrewCareer.Infrastructure.Tests
{
    public class TestQuestionConfigurations : IEntityTypeConfiguration<TestQuestion>
    {
        public void Configure(EntityTypeBuilder<TestQuestion> builder)
        {
            builder.ToTable("TestQuestion");

            builder.HasKey(n => n.QuestionId);

            builder.Property(n => n.QuestionContent)
                .HasMaxLength(50)
                .IsRequired();

            builder.HasOne(u => u.Test)
                .WithMany(u => u.TestQuestions)
                .HasForeignKey(u => u.TestId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
