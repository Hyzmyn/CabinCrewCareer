using CabinCrewCareer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CabinCrewCareer.Infrastructure.Configurations
{
    public class SystemConfigurations : IEntityTypeConfiguration<RoundConfiguration>
    {
        public void Configure(EntityTypeBuilder<RoundConfiguration> builder)
        {
            builder.ToTable("RoundConfiguration");

            builder.HasKey(c => c.RoundConfigurationId);

            builder.HasOne(w => w.RoundType)
                .WithMany(w => w.RoundConfigurations)
                .HasForeignKey(w => w.RoundTypeId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
