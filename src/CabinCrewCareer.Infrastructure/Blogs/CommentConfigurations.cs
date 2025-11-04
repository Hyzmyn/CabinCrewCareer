using CabinCrewCareer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CabinCrewCareer.Infrastructure.Blogs
{
    public class CommentConfigurations : IEntityTypeConfiguration<Comment>
    {
        public void Configure(EntityTypeBuilder<Comment> builder)
        {
            builder.ToTable("Comment");

            builder.HasKey(c => c.CommentId);

            builder.Property(c => c.CommentBody)
                .IsRequired()
                .HasMaxLength(100);

            builder.HasOne(c => c.User)
                .WithMany(c => c.Comments)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(c => c.Blog)
                .WithMany(c => c.Comments)
                .HasForeignKey(c => c.BlogId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
