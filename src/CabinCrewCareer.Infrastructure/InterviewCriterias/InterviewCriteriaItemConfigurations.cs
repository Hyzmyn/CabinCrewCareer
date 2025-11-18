using CabinCrewCareer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CabinCrewCareer.Infrastructure.InterviewCriterias
{
    public class InterviewCriteriaItemConfigurations : IEntityTypeConfiguration<InterviewCriteriaItem>
    {
        public void Configure(EntityTypeBuilder<InterviewCriteriaItem> builder)
        {
            builder.ToTable("InterviewCriteriaItem");

            builder.HasKey(n => n.InterviewCriteriaItemId);

            builder.Property(n => n.Criteria)
                .HasMaxLength(250);

            builder.HasOne(n => n.InterviewCriteria)
                .WithMany(n => n.InterviewCriteriaItems)
                .HasForeignKey(n => n.InterviewCriteriaId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
