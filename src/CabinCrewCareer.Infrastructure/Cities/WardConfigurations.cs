using CabinCrewCareer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CabinCrewCareer.Infrastructure.Cities
{
    public class WardConfigurations : IEntityTypeConfiguration<Ward>
    {
        public void Configure(EntityTypeBuilder<Ward> builder)
        {
            builder.ToTable("Ward");

            builder.HasKey(w => w.WardId);

            builder.Property(w => w.WardName)
                .IsRequired()
                .HasMaxLength(50);

            builder.HasOne(w => w.City)
                .WithMany(w => w.Wards)
                .HasForeignKey(w => w.CityId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
