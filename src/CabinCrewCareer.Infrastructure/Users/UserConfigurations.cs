using CabinCrewCareer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CabinCrewCareer.Infrastructure.Users
{
    public class UserConfigurations : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("User");

            builder.HasKey(u => u.UserId);

            builder.Property(u => u.UserName)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(u => u.FullName)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(u => u.Password)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(u => u.PhoneNumber)
                .IsRequired()
                .HasMaxLength(11);

            builder.Property(u => u.Address)
                .HasMaxLength(50);

            builder.Property(u => u.ResetPasswordOtp)
                .HasMaxLength(6);

            builder.HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(u => u.AirlinePartner)
                .WithMany(u => u.Users)
                .HasForeignKey(u => u.PartnerId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(u => u.Ward)
                .WithMany(u => u.Users)
                .HasForeignKey(u => u.WardId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(u => u.Application)
                .WithOne(u => u.User)
                .HasForeignKey<UserApplication>(u => u.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
