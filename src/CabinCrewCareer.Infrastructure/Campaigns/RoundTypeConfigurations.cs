using CabinCrewCareer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CabinCrewCareer.Infrastructure.Campaigns
{
    public class RoundTypeConfigurations : IEntityTypeConfiguration<TypeRound>
    {
        public void Configure(EntityTypeBuilder<TypeRound> builder)
        {
            builder.ToTable("RoundType");

            builder.HasKey(n => n.RoundTypeId);

            builder.Property(n => n.RoundTypeName)
                .HasMaxLength(50)
                .IsRequired();
        }
    }
}
