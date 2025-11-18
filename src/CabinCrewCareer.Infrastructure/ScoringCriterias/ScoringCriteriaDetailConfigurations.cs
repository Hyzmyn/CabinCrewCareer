using CabinCrewCareer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CabinCrewCareer.Infrastructure.ScoringCriterias
{
    public class ScoringCriteriaDetailConfigurations : IEntityTypeConfiguration<ScoringCriteriaDetail>
    {
        public void Configure(EntityTypeBuilder<ScoringCriteriaDetail> builder)
        {
            builder.ToTable("ScoringCriteriaDetail");

            builder.HasKey(n => n.ScoringCriteriaDetailId);

            builder.Property(n => n.DetailText)
                .HasMaxLength(100);

            builder.HasOne(n => n.ScoringCriteriaItem)
                .WithMany(n => n.ScoringCriteriaDetails)
                .HasForeignKey(n => n.ScoringCriteriaItemId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
