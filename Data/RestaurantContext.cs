using Microsoft.EntityFrameworkCore;
using Restaurant.Models;

namespace Restaurant.Data
{
    public class RestaurantContext : DbContext
    {
        public RestaurantContext(DbContextOptions<RestaurantContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Status> Statuses { get; set; }
        public DbSet<Food> Foods { get; set; }
        public DbSet<FoodType> FoodTypes { get; set; }
        public DbSet<OrderFood> OrderFoods { get; set; }
        public DbSet<Place> Places { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<Role>().ToTable("Role");

            modelBuilder.Entity<Order>()
                .ToTable("Order")
                .HasOne(x => x.Waiter)
                .WithMany(x => x.Orders)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Status>().ToTable("Status");
            modelBuilder.Entity<Food>().ToTable("Food");
            modelBuilder.Entity<FoodType>().ToTable("FoodType");
            modelBuilder.Entity<OrderFood>().ToTable("OrderFood");
            modelBuilder.Entity<Place>().ToTable("Place");

            // Зайві таблиці від шаблону (Product, Recipe, Provider, ProductType) видалені 
            // з OnModelCreating, щоб не виникало помилок при збірці проекту.
        }
    }
}