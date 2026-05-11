using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore; // Додано для Include
using Restaurant.Data;
using Restaurant.Models;
using Restaurant.Extensions;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Restaurant.Controllers
{
    public class CartController : Controller
    {
        private readonly RestaurantContext _context;

        public CartController(RestaurantContext context)
        {
            _context = context;
        }

        // 1. Сторінка "Мої замовлення"
        public IActionResult MyOrders()
        {
            var userName = User.Identity.Name;

            // Шукаємо користувача ТІЛЬКИ за логіном (без Email)
            var user = _context.Users.FirstOrDefault(u => u.Login == userName);

            if (user == null) return Unauthorized();

            var orders = _context.Orders
                .Include(o => o.Status)
                .Include(o => o.Place)
                .Include(o => o.OrderFoods)
                    .ThenInclude(of => of.Food)
                .Where(o => o.ClientID == user.ID)
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            return View(orders);
        }

        // 2. Головна сторінка кошика
        public IActionResult Index()
        {
            var cart = HttpContext.Session.GetJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            ViewBag.Places = new SelectList(_context.Places, "ID", "Name");
            return View(cart);
        }

        // 3. Додавання в кошик
        [HttpPost]
        public IActionResult AddToCart(int id)
        {
            var food = _context.Foods.Find(id);
            if (food == null) return NotFound();

            var cart = HttpContext.Session.GetJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            var item = cart.FirstOrDefault(x => x.FoodID == id);

            if (item == null)
            {
                cart.Add(new CartItem { FoodID = id, Name = food.Name, Price = food.Price, Quantity = 1 });
            }
            else
            {
                item.Quantity++;
            }

            HttpContext.Session.SetJson("Cart", cart);
            return Json(new { count = cart.Sum(x => x.Quantity) });
        }

        // 4. Оновлення кількості
        [HttpPost]
        public IActionResult UpdateQuantity(int id, int delta)
        {
            var cart = HttpContext.Session.GetJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            var item = cart.FirstOrDefault(x => x.FoodID == id);

            if (item != null)
            {
                item.Quantity += delta;
                if (item.Quantity <= 0) cart.Remove(item);
                HttpContext.Session.SetJson("Cart", cart);
            }

            return Json(new
            {
                count = cart.Sum(x => x.Quantity),
                total = cart.Sum(x => x.Price * x.Quantity),
                itemCount = item?.Quantity ?? 0,
                itemSubtotal = item != null ? item.Price * item.Quantity : 0
            });
        }

        // 5. Видалення з кошика
        [HttpPost]
        public IActionResult RemoveFromCart(int id)
        {
            var cart = HttpContext.Session.GetJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            var item = cart.FirstOrDefault(x => x.FoodID == id);

            if (item != null)
            {
                cart.Remove(item);
                HttpContext.Session.SetJson("Cart", cart);
            }

            return Json(new { count = cart.Sum(x => x.Quantity), total = cart.Sum(x => x.Price * x.Quantity) });
        }

        // 6. Оформлення замовлення
        [HttpPost]
        public IActionResult Checkout(int placeId)
        {
            var cart = HttpContext.Session.GetJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            if (!cart.Any()) return RedirectToAction("Index");

            var userName = User.Identity.Name;

            // Шукаємо за Login (це поле 100% є в базі)
            var user = _context.Users.FirstOrDefault(u => u.Login == userName);

            if (user == null) return Unauthorized();

            var order = new Order
            {
                ClientID = user.ID,
                OrderDate = DateTime.Now,
                Price = (int)cart.Sum(x => x.Price * x.Quantity),
                PlaceID = placeId,
                StatusID = 1, // "Готується"
                OrderFoods = cart.Select(i => new OrderFood
                {
                    FoodID = i.FoodID,
                    Quantity = i.Quantity
                }).ToList()
            };

            _context.Orders.Add(order);
            _context.SaveChanges();

            HttpContext.Session.Remove("Cart");

            return View("OrderSuccess", cart);
        }
    }
}