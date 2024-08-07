﻿using MealApp.Models;
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

        public DbSet<Coupon> coupons { get;set;}

        public DbSet<Contect> contects { get;set;}

        public DbSet<Feedback> feedbacks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().ToTable("users");
            modelBuilder.Entity<Booking>().ToTable("booking");
            modelBuilder.Entity<Notification>().ToTable("notification");
            modelBuilder.Entity<Coupon>().ToTable("coupon");
            modelBuilder.Entity<Contect>().ToTable("contect");
            modelBuilder.Entity<Feedback>().ToTable("feedback");
        }
        
    }
}
