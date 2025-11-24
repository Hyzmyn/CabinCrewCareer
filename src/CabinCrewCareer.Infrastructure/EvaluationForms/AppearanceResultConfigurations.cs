using CabinCrewCareer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CabinCrewCareer.Infrastructure.EvaluationForms
{
    public class AppearanceResultConfigurations : IEntityTypeConfiguration<AppearanceResult>
    {
        public void Configure(EntityTypeBuilder<AppearanceResult> builder)
        {
            builder.ToTable("AppearanceResult");

            builder.HasKey(n => n.AppearanceResultId);

            builder.HasOne(n => n.EvaluationForm)
                .WithMany(n => n.AppearanceResults)
                .HasForeignKey(n => n.EvaluationId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(n => n.ScoringCriteriaItem)
                .WithMany(n => n.AppearanceResults)
                .HasForeignKey(n => n.ScoringCriteriaItemId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
