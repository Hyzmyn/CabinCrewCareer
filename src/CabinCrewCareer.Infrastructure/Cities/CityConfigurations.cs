using CabinCrewCareer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CabinCrewCareer.Infrastructure.Cities
{
    public class CityConfigurations : IEntityTypeConfiguration<City>
    {
        public void Configure(EntityTypeBuilder<City> builder)
        {
            builder.ToTable("City");

            builder.HasKey(c => c.CityId);

            builder.Property(c => c.CityName)
                .IsRequired()
                .HasMaxLength(25);
        }
    }
}
