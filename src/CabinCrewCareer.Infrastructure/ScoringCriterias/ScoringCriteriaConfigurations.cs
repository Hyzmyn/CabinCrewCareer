using CabinCrewCareer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CabinCrewCareer.Infrastructure.ScoringCriterias
{
    public class ScoringCriteriaConfigurations : IEntityTypeConfiguration<ScoringCriteria>
    {
        public void Configure(EntityTypeBuilder<ScoringCriteria> builder)
        {
            builder.ToTable("ScoringCriteria");

            builder.HasKey(n => n.ScoringCriteriaId);

            builder.Property(n => n.Title)
                .HasMaxLength(50);
        }
    }
}
