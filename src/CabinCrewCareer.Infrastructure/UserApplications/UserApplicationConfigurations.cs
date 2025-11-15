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

            //builder.HasOne(n => n.Round)
            //    .WithMany(n => n.Applications)
            //    .HasForeignKey(n => n.RoundId)
            //    .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
