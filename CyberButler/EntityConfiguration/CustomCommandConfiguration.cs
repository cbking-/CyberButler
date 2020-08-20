using CyberButler.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace CyberButler.EntityConfiguration
{
    class CustomCommandConfiguration : IEntityTypeConfiguration<CustomCommand>
    {
        public void Configure(EntityTypeBuilder<CustomCommand> builder)
        {
            builder.ToTable("CustomCommand");

            builder.HasKey(k => new { k.Server, k.Command });

            builder.Property(p => p.Server).IsRequired();
            builder.Property(p => p.Command).IsRequired();
            builder.Property(p => p.Text).IsRequired();
        }
    }
}
