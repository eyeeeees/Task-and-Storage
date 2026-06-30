using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;
using Npgsql;
using tas.Models;

namespace tas.Data
{
    public class TasDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Status> Statuses { get; set; }
        public DbSet<AppTask> Tasks { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=tas;Username=postgres;Password=podshoev_D2008");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
         
            modelBuilder.Entity<Status>().HasData(
                new Status { Id = 1, Name = "Новая" },
                new Status { Id = 2, Name = "В работе" },
                new Status { Id = 3, Name = "Завершена" },
                new Status { Id = 4, Name = "Отменена" }
            );
        }
    }
}