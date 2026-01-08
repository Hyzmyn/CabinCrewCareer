using CabinCrewCareer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CabinCrewCareer.Infrastructure.Configurations
{
    public class LocationConfigurations : IEntityTypeConfiguration<Location>
    {
        public void Configure(EntityTypeBuilder<Location> builder)
        {
            builder.ToTable("Location");

            builder.HasKey(c => c.LocationId);

            builder.Property(u => u.Address)
                .HasMaxLength(50);

            builder.HasOne(w => w.Ward)
                .WithMany(w => w.Locations)
                .HasForeignKey(w => w.WardId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
