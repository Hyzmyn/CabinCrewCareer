using CabinCrewCareer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CabinCrewCareer.Infrastructure.UserApplications
{
    public class DocumentConfigurations : IEntityTypeConfiguration<Document>
    {
        public void Configure(EntityTypeBuilder<Document> builder)
        {
            builder.ToTable("Document");

            builder.HasKey(n => n.DocumentId);

            builder.Property(n => n.DocumentURL)
                .IsRequired();

            builder.HasOne(n => n.Application)
                .WithMany(n => n.Documents)
                .HasForeignKey(n => n.ApplicationId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
