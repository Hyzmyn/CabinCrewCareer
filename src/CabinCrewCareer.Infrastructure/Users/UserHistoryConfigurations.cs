using CabinCrewCareer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CabinCrewCareer.Infrastructure.Users
{
    public class UserHistoryConfigurations : IEntityTypeConfiguration<UserHistory>
    {
        public void Configure(EntityTypeBuilder<UserHistory> builder)
        {
            builder.ToTable("UserHistory");

            builder.HasKey(u => u.HistoryId);

            builder.HasOne(u => u.CampaignRound)
                .WithMany(u => u.Histories)
                .HasForeignKey(u => u.CampaignRoundId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(u => u.User)
                .WithMany(u => u.Histories)
                .HasForeignKey(u => u.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
