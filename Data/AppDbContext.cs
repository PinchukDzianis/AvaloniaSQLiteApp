using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Mode> Modes { get; set; }
        public DbSet<Step> Steps { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Login)
                .IsUnique();

            modelBuilder.Entity<Mode>()
                .Property(m => m.MaxBottleNumber)
                .IsRequired(false);

            modelBuilder.Entity<Step>()
                .HasOne(s => s.Mode)
                .WithMany(m => m.Steps)
                .HasForeignKey(s => s.ModeId);
        }
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            string dbPath = Path.Combine(AppContext.BaseDirectory, "database.db");
            options.UseSqlite($"Data Source={dbPath}");
        }
    }
}
