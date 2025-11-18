using CabinCrewCareer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CabinCrewCareer.Infrastructure.InterviewCriterias
{
    public class InterviewCriteriaConfigurations : IEntityTypeConfiguration<InterviewCriteria>
    {
        public void Configure(EntityTypeBuilder<InterviewCriteria> builder)
        {
            builder.ToTable("InterviewCriteria");

            builder.HasKey(n => n.InterviewCriteriaId);

            builder.Property(n => n.Title)
                .HasMaxLength(50);
        }
    }
}
