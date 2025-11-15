using CabinCrewCareer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CabinCrewCareer.Infrastructure.Campaigns
{
    public class RoundActivityConfigurations : IEntityTypeConfiguration<RoundActivity>
    {
        public void Configure(EntityTypeBuilder<RoundActivity> builder)
        {
            builder.ToTable("RoundActivity");

            builder.HasKey(n => n.ActivityId);

            builder.HasOne(n => n.Round)
                .WithMany(n => n.RoundActivities)
                .HasForeignKey(n => n.RoundId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(n => n.User)
                .WithMany(n => n.RoundActivities)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
