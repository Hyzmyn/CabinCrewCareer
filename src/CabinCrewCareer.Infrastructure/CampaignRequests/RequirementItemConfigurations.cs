using CabinCrewCareer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CabinCrewCareer.Infrastructure.CampaignRequests
{
    public class RequirementItemConfigurations : IEntityTypeConfiguration<RequirementItem>
    {
        public void Configure(EntityTypeBuilder<RequirementItem> builder)
        {
            builder.ToTable("RequirementItem");

            builder.HasKey(n => n.RequirementItemId);

            builder.Property(n => n.Title)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(n => n.Description)
                .HasMaxLength(150)
                .IsRequired();

            builder.HasOne(u => u.Requirement)
                .WithMany(u => u.RequirementItems)
                .HasForeignKey(u => u.RequirementId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
