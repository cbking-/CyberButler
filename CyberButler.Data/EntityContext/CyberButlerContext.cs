using CyberButler.Data.Entities;
using CyberButler.Data.EntityConfiguration;
using Microsoft.EntityFrameworkCore;

namespace CyberButler.Data.EntityContext
{
    public class CyberButlerContext : DbContext
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
                optionsBuilder.UseSqlite(@"Data Source=D:\home\site\wwwroot\Database.sqlite3");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new CustomCommandConfiguration());
            modelBuilder.ApplyConfiguration(new UsernameHistoryConfiguration());
        }
    }
}
