using CabinCrewCareer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CabinCrewCareer.Infrastructure.Notifications
{
    public class NotificationTypeConfigurations : IEntityTypeConfiguration<NotificationType>
    {
        public void Configure(EntityTypeBuilder<NotificationType> builder)
        {
            builder.ToTable("NotificationType");

            builder.HasKey(n => n.NotificationTypeId);

            builder.Property(n => n.NotificationTypeName)
                .IsRequired()
                .HasMaxLength(25);

            builder.Property(n => n.Description)
                .HasMaxLength(50);
        }
    }
}
