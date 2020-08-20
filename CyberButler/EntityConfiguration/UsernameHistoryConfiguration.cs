using CyberButler.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CyberButler.EntityConfiguration
{
    class UsernameHistoryConfiguration : IEntityTypeConfiguration<UsernameHistory>
    {
        public void Configure(EntityTypeBuilder<UsernameHistory> builder)
        {
            builder.ToTable("UsernameHistory");
            builder.HasKey(k => new { k.Server, k.UserID, k.NameBefore, k.NameAfter, k.InsertDateTime });

            builder.Property(p => p.Server).IsRequired();
            builder.Property(p => p.UserID).IsRequired();
            builder.Property(p => p.NameBefore).IsRequired();
            builder.Property(p => p.NameAfter).IsRequired();
            builder.Property(p => p.InsertDateTime).IsRequired();
        }
    }
}
