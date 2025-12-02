using CabinCrewCareer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CabinCrewCareer.Infrastructure.UserApplications
{
    public class DocumentSnapshotConfigurations : IEntityTypeConfiguration<DocumentSnapshot>
    {
        public void Configure(EntityTypeBuilder<DocumentSnapshot> builder)
        {
            builder.ToTable("DocumentSnapshot");

            builder.HasKey(n => n.DocumentSnapshotId);

            builder.Property(n => n.DocumentURL)
                .IsRequired();

            builder.HasOne(n => n.UserApplicationSnapshot)
                .WithMany(n => n.DocumentSnapshots)
                .HasForeignKey(n => n.ApplicationSnapshotId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
