using CabinCrewCareer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CabinCrewCareer.Infrastructure.EvaluationForms
{
    public class InterviewResultConfigurations : IEntityTypeConfiguration<InterviewResult>
    {
        public void Configure(EntityTypeBuilder<InterviewResult> builder)
        {
            builder.ToTable("InterviewResult");

            builder.HasKey(n => n.InterviewResultId);

            builder.Property(n => n.Comment)
                .HasMaxLength(150);

            builder.HasOne(n => n.EvaluationForm)
                .WithMany(n => n.InterviewResults)
                .HasForeignKey(n => n.EvaluationId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(n => n.InterviewCriteriaItem)
                .WithMany(n => n.InterviewResults)
                .HasForeignKey(n => n.InterviewCriteriaItemId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
