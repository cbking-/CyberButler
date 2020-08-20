using CyberButler.Entities;
using CyberButler.EntityConfiguration;
using Microsoft.EntityFrameworkCore;
using System;

namespace CyberButler.EntityContext
{
    class CyberButlerContext : DbContext
    {
        public CyberButlerContext() { }

        public CyberButlerContext(DbContextOptions<CyberButlerContext> options)
            : base(options) { }

        public virtual DbSet<CustomCommand> CustomCommand { get; set; }
        public virtual DbSet<UsernameHistory> UsernameHistory { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite("Data Source=Database.sqlite3");                
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new CustomCommandConfiguration());
            modelBuilder.ApplyConfiguration(new UsernameHistoryConfiguration());
        }
    }
}
