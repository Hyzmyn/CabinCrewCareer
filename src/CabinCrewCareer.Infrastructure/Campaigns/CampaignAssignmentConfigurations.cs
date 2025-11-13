using CabinCrewCareer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CabinCrewCareer.Infrastructure.Campaigns
{
    public class CampaignAssignmentConfigurations : IEntityTypeConfiguration<CampaignAssignment>
    {
        public void Configure(EntityTypeBuilder<CampaignAssignment> builder)
        {
            builder.ToTable("CampaignAssignment");

            builder.HasKey(n => n.AssignmentId);

            builder.Property(u => u.TaskDescription)
                .HasMaxLength(100);

            builder.Property(u => u.AssignedBy)
                .HasMaxLength(20);

            builder.Property(u => u.Notes)
                .HasMaxLength(250);

            builder.HasOne(n => n.User)
                .WithMany(n => n.Assignments)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(n => n.Campaign)
                .WithMany(n => n.Assignments)
                .HasForeignKey(n => n.CampaignId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
