using CabinCrewCareer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CabinCrewCareer.Infrastructure.Campaigns
{
    public class CampaignConfigurations : IEntityTypeConfiguration<Campaign>
    {
        public void Configure(EntityTypeBuilder<Campaign> builder)
        {
            builder.ToTable("Campaign");

            builder.HasKey(n => n.CampaignId);

            builder.Property(u => u.CampaignName)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(u => u.ReviewedBy)
                .HasMaxLength(20);

            builder.Property(u => u.RejectedReason)
                .HasMaxLength(250);

            builder.Property(u => u.Description)
                .HasMaxLength(50);

            builder.HasOne(n => n.SeniorRecruiter)
                .WithMany(n => n.Campaigns)
                .HasForeignKey(n => n.SeniorId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(n => n.CampaignRequest)
                .WithMany(n => n.Campaigns)
                .HasForeignKey(n => n.RequestId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
