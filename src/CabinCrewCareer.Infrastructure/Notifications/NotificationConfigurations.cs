using CabinCrewCareer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CabinCrewCareer.Infrastructure.Notifications
{
    public class NotificationConfigurations : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            builder.ToTable("Notification");

            builder.HasKey(n => n.NotificationId);

            builder.Property(n => n.Title)
                .IsRequired()
                .HasMaxLength(25);

            builder.Property(n => n.Body)
                .IsRequired()
                .HasMaxLength(100);

            builder.HasOne(n => n.NotificationType)
                .WithMany(n => n.Notifications)
                .HasForeignKey(n => n.NotificationTypeId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(n => n.User)
                .WithMany(n => n.Notifications)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
