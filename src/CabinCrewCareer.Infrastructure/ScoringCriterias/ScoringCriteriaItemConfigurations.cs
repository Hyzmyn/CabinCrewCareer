using CabinCrewCareer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CabinCrewCareer.Infrastructure.ScoringCriterias
{
    public class ScoringCriteriaItemConfigurations : IEntityTypeConfiguration<ScoringCriteriaItem>
    {
        public void Configure(EntityTypeBuilder<ScoringCriteriaItem> builder)
        {
            builder.ToTable("ScoringCriteriaItem");

            builder.HasKey(n => n.ScoringCriteriaItemId);

            builder.Property(n => n.ItemType)
                .HasMaxLength(50);

            builder.Property(n => n.Text)
                .HasMaxLength(250);

            builder.Property(n => n.EnglishText)
                .HasMaxLength(250);

            builder.HasOne(n => n.ScoringCriteria)
                .WithMany(n => n.ScoringCriteriaItems)
                .HasForeignKey(n => n.ScoringCriteriaId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
