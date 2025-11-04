using CabinCrewCareer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CabinCrewCareer.Infrastructure.Campaigns
{
    public class CampaignRequestConfigurations : IEntityTypeConfiguration<CampaignRequest>
    {
        public void Configure(EntityTypeBuilder<CampaignRequest> builder)
        {
            builder.ToTable("CampaignRequest");

            builder.HasKey(n => n.RequestId);

            builder.Property(u => u.CampaignName)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(u => u.Description)
                .IsRequired()
                .HasMaxLength(50);

            builder.HasOne(n => n.AirlinePartner)
                .WithMany(n => n.CampaignRequests)
                .HasForeignKey(n => n.PartnerId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(n => n.Director)
                .WithMany(n => n.CampaignRequests)
                .HasForeignKey(n => n.DirectorId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
