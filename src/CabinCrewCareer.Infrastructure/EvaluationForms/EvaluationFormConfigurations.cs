using CabinCrewCareer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CabinCrewCareer.Infrastructure.EvaluationForms
{
    public class EvaluationFormConfigurations : IEntityTypeConfiguration<EvaluationForm>
    {
        public void Configure(EntityTypeBuilder<EvaluationForm> builder)
        {
            builder.ToTable("EvaluationForm");

            builder.HasKey(n => n.EvaluationId);

            builder.Property(n => n.Comment)
                .HasMaxLength(100);

            builder.HasOne(n => n.RoundActivity)
                .WithMany(n => n.EvaluationForms)
                .HasForeignKey(n => n.ActivityId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(n => n.Examiner)
                .WithMany(n => n.EvaluationForms)
                .HasForeignKey(n => n.ExaminerId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
