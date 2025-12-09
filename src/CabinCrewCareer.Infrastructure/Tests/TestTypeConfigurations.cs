using CabinCrewCareer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CabinCrewCareer.Infrastructure.Tests
{
    public class TestTypeConfigurations : IEntityTypeConfiguration<TypeTest>
    {
        public void Configure(EntityTypeBuilder<TypeTest> builder)
        {
            builder.ToTable("TestType");

            builder.HasKey(n => n.TestTypeId);

            builder.Property(n => n.TestTypeName)
                .HasMaxLength(50)
                .IsRequired();
        }
    }
}
