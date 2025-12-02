using CabinCrewCareer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CabinCrewCareer.Infrastructure.UserApplications
{
    public class UserApplicationSnapshotConfigurations : IEntityTypeConfiguration<UserApplicationSnapshot>
    {
        public void Configure(EntityTypeBuilder<UserApplicationSnapshot> builder)
        {
            builder.ToTable("UserApplicationSnapshot");

            builder.HasKey(n => n.ApplicationSnapshotId);

            builder.Property(n => n.Experience)
                .HasMaxLength(20);

            builder.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(u => u.FullName)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(u => u.PhoneNumber)
                .IsRequired()
                .HasMaxLength(11);

            builder.Property(n => n.EnglishDegreeNumber)
                .HasMaxLength(50);

            builder.Property(n => n.CitizenId)
                .HasMaxLength(20);
        }
    }
}
