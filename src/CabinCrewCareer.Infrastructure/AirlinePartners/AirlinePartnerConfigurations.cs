using CabinCrewCareer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CabinCrewCareer.Infrastructure.AirlinePartners
{
    public class AirlinePartnerConfigurations : IEntityTypeConfiguration<AirlinePartner>
    {
        public void Configure(EntityTypeBuilder<AirlinePartner> builder)
        {
            builder.ToTable("AirlinePartner");

            builder.HasKey(w => w.PartnerId);

            builder.Property(u => u.PartnerName)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(u => u.Description)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(u => u.Address)
                .HasMaxLength(50);

            builder.HasOne(w => w.Ward)
                .WithMany(w => w.AirlinePartners)
                .HasForeignKey(w => w.WardId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
