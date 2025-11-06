using CabinCrewCareer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CabinCrewCareer.Infrastructure.Tests
{
    public class TestOptionConfigurations : IEntityTypeConfiguration<TestOption>
    {
        public void Configure(EntityTypeBuilder<TestOption> builder)
        {
            builder.ToTable("TestOption");

            builder.HasKey(n => n.OptionId);

            builder.Property(n => n.OptionContent)
                .HasMaxLength(50)
                .IsRequired();

            builder.HasOne(u => u.TestQuestion)
                .WithMany(u => u.TestOptions)
                .HasForeignKey(u => u.QuestionId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(u => u.TestAnswer)
                .WithOne(u => u.SelectedOption)
                .HasForeignKey<TestAnswer>(u => u.SelectedOptionId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
