using CabinCrewCareer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CabinCrewCareer.Infrastructure.UserApplications
{
    public class UserApplicationConfigurations : IEntityTypeConfiguration<UserApplication>
    {
        public void Configure(EntityTypeBuilder<UserApplication> builder)
        {
            builder.ToTable("UserApplication");

            builder.HasKey(n => n.ApplicationId);

            builder.Property(n => n.Experience)
                .HasMaxLength(20);

            builder.Property(n => n.EnglishDegreeNumber)
                .HasMaxLength(50);

            builder.Property(n => n.CitizenId)
                .HasMaxLength(20);

            builder.Property(n => n.EnglishTestDate);
            builder.Property(n => n.ListeningScore);
            builder.Property(n => n.ReadingScore);
            builder.Property(n => n.TotalScore);

            //builder.HasOne(n => n.Round)
            //    .WithMany(n => n.Applications)
            //    .HasForeignKey(n => n.RoundId)
            //    .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
