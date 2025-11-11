using CabinCrewCareer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CabinCrewCareer.Infrastructure.Campaigns
{
    public class CampaignRoundConfigurations : IEntityTypeConfiguration<CampaignRound>
    {
        public void Configure(EntityTypeBuilder<CampaignRound> builder)
        {
            builder.ToTable("CampaignRound");

            builder.HasKey(n => n.CampaignRoundId);

            builder.Property(u => u.RoundName)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(u => u.Description)
                .HasMaxLength(50);

            builder.HasOne(n => n.Campaign)
                .WithMany(n => n.CampaignRounds)
                .HasForeignKey(n => n.CampaignId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
