using CabinCrewCareer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CabinCrewCareer.Infrastructure.Campaigns
{
    public class RoundConfigurations : IEntityTypeConfiguration<Round>
    {
        public void Configure(EntityTypeBuilder<Round> builder)
        {
            builder.ToTable("Round");

            builder.HasKey(n => n.RoundId);

            builder.Property(u => u.Notes)
                .HasMaxLength(50);

            builder.HasOne(n => n.CampaignRound)
                .WithMany(n => n.Rounds)
                .HasForeignKey(n => n.CampaignRoundId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(n => n.Test)
                .WithMany(n => n.Rounds)
                .HasForeignKey(n => n.TestId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
