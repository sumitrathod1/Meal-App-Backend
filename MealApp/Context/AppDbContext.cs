using MealApp.Models;
using Microsoft.EntityFrameworkCore;

namespace MealApp.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        public DbSet<User> Users {get;set;}
        public DbSet<Booking> Bookings { get;set;}

        public DbSet<Notification> notifications { get;set;}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().ToTable("users");
            modelBuilder.Entity<Booking>().ToTable("booking");
            modelBuilder.Entity<Notification>().ToTable("notification");
        }
        
    }
}
