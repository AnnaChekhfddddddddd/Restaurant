using Restaurant.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Restaurant.Data
{
    public class DBInitializer
    {
        public static void Initialize(RestaurantContext context)
        {
            context.Database.Migrate();

            // ---------------- ROLES ----------------
            if (!context.Roles.Any())
            {
                var roles = new Role[]
                {
                    new Role { Name = "Менеджер" },
                    new Role { Name = "Офіціант" },
                    new Role { Name = "Клієнт" }
                };
                context.Roles.AddRange(roles);
                context.SaveChanges();
            }

            // ---------------- STATUSES ----------------
            if (!context.Statuses.Any())
            {
                var statuses = new Status[]
                {
                    new Status { Name = "Відкритий" },
                    new Status { Name = "Активний" },
                    new Status { Name = "Завершений" }
                };
                context.Statuses.AddRange(statuses);
                context.SaveChanges();
            }

            // ---------------- FOOD TYPES ----------------
            if (!context.FoodTypes.Any())
            {
                var types = new FoodType[]
                {
                    new FoodType{ Name = "Перші страви"},
                    new FoodType{ Name = "Основні страви"},
                    new FoodType{ Name = "Гарячі закуски"},
                    new FoodType{ Name = "Гарніри"},
                    new FoodType{ Name = "Випічка"},
                    new FoodType{ Name = "Десерти"},
                    new FoodType{ Name = "Безалкогольні/Алкогольні напої"},
                    new FoodType{ Name = "Холодні закуски"},
                    new FoodType{ Name = "Салати"}
                };
                context.FoodTypes.AddRange(types);
                context.SaveChanges();
            }

            // ---------------- USERS ----------------
            if (!context.Users.Any())
            {
                var users = new User[]
                {
                    new User{ FirstName="Олександр", LastName="Карпенко", Phone="+380969379992", Avatar="/img/manager_avatar.png", RoleID=context.Roles.First(x=>x.Name=="Менеджер").ID, Login="manager", Password="manager" },
                    new User{ FirstName="Іван", LastName="Гамурар", Phone="+380971255284", Avatar="/img/waiter_avatar.png", RoleID=context.Roles.First(x=>x.Name=="Офіціант").ID, Login="waiter1", Password="waiter1" },
                    new User{ FirstName="Сергій", LastName="Іванов", Phone="+380505553535", Avatar="/img/client_avatar.png", RoleID=context.Roles.First(x=>x.Name=="Клієнт").ID, Login="client", Password="client" }
                };
                context.Users.AddRange(users);
                context.SaveChanges();
            }

            // ---------------- PLACES ----------------
            if (!context.Places.Any())
            {
                var places = new Place[]
                {
                    new Place{Name="Столик 1"}, new Place{Name="Столик 2"}, new Place{Name="Столик 3"},
                    new Place{Name="Столик 4"}, new Place{Name="Столик 5"}, new Place{Name="Столик 6"}
                };
                context.Places.AddRange(places);
                context.SaveChanges();
            }

            // ---------------- FOODS ----------------
            if (!context.Foods.Any())
            {
                var foods = new Food[]
                {
                    new Food { Name = "Борщ український з салом", Price = 145, Image = "/img/food.png", FoodTypeID = context.FoodTypes.First(x => x.Name == "Перші страви").ID, IsAvailable = true },
                    new Food { Name = "Стейк Рибай", Price = 550, Image = "/img/food.png", FoodTypeID = context.FoodTypes.First(x => x.Name == "Основні страви").ID, IsAvailable = true },
                    new Food { Name = "Тірамісу", Price = 150, Image = "/img/food.png", FoodTypeID = context.FoodTypes.First(x => x.Name == "Десерти").ID, IsAvailable = true },
                    new Food { Name = "Кава Капучино", Price = 65, Image = "/img/food.png", FoodTypeID = context.FoodTypes.First(x => x.Name == "Безалкогольні/Алкогольні напої").ID, IsAvailable = true }
                };

                context.Foods.AddRange(foods);
                context.SaveChanges();
            }
        }
    }
}