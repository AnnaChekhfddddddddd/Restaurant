using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Restaurant.Data;
using Restaurant.Models;
using Microsoft.AspNetCore.Authorization;

namespace Restaurant.Controllers
{
    public class OrdersController : Controller
    {
        private readonly RestaurantContext _context;

        public OrdersController(RestaurantContext context)
        {
            _context = context;
        }

        // ==========================================
        // 1. ГОЛОВНА СТОРІНКА (ФІЛЬТРАЦІЯ ЗА ДАТОЮ ТА СОРТУВАННЯ)
        // ==========================================
        public async Task<IActionResult> Index(DateTime? SelectedDate)
        {
            var currentUser = await _context.Users
                .Include(x => x.Role)
                .FirstOrDefaultAsync(x => x.Login.Equals(HttpContext.User.Identity.Name));

            if (currentUser == null) return NotFound();

            // Базовий запит з усіма необхідними зв'язками
            var query = _context.Orders
                .Include(o => o.Client)
                .Include(o => o.Status)
                .Include(o => o.Waiter)
                .Include(o => o.Place).ThenInclude(p => p.Waiter)
                .AsQueryable();

            // Фільтрація за конкретною датою (наприклад, "за вчора")
            if (SelectedDate.HasValue)
            {
                DateTime start = SelectedDate.Value.Date; // 00:00:00
                DateTime end = start.AddDays(1).AddTicks(-1); // 23:59:59
                query = query.Where(x => x.OrderDate >= start && x.OrderDate <= end);
                ViewBag.SelectedDate = SelectedDate.Value.ToString("yyyy-MM-dd");
            }

            // Розрахунок прибутку для відфільтрованого списку
            ViewBag.Profit = await query.SumAsync(x => x.Price);

            // Сортування: нові замовлення зверху (від №7 до №1)
            query = query.OrderByDescending(x => x.OrderDate);

            List<Order> result;

            if (currentUser.Role.Name.Equals("Менеджер"))
            {
                result = await query.ToListAsync();
            }
            else if (currentUser.Role.Name.Equals("Офіціант"))
            {
                // Офіціант бачить свої замовлення або замовлення на своїх столиках
                result = await query.Where(x => x.WaiterID == currentUser.ID
                                             || x.Place.WaiterID == currentUser.ID
                                             || x.WaiterID == null).ToListAsync();
            }
            else
            {
                // Клієнт бачить тільки своє
                result = await query.Where(x => x.ClientID == currentUser.ID).ToListAsync();
            }

            return View(result);
        }

        // ==========================================
        // 2. ДЕТАЛІ ЗАМОВЛЕННЯ
        // ==========================================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var order = await _context.Orders
                .Include(o => o.Client)
                .Include(o => o.Place).ThenInclude(p => p.Waiter)
                .Include(o => o.Status)
                .Include(o => o.Waiter)
                .Include(o => o.OrderFoods)
                .FirstOrDefaultAsync(m => m.ID == id);

            if (order == null) return NotFound();

            foreach (var orderFood in order.OrderFoods)
            {
                orderFood.Food = await _context.Foods.FirstAsync(x => x.ID == orderFood.FoodID);
            }

            return View(order);
        }

        // ==========================================
        // 3. СТВОРЕННЯ ЗАМОВЛЕННЯ (АВТО-ПРИЗНАЧЕННЯ ОФІЦІАНТА)
        // ==========================================
        public IActionResult Create()
        {
            var currentUser = _context.Users.First(x => x.Login.Equals(HttpContext.User.Identity.Name));
            ViewData["ClientID"] = new SelectList(_context.Users, "ID", "FullName", currentUser.ID);
            ViewData["PlaceID"] = new SelectList(_context.Places, "ID", "Name");
            ViewData["StatusID"] = new SelectList(_context.Statuses, "ID", "Name", _context.Statuses.First(x => x.Name.Equals("Відкритий")).ID);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ClientID,StatusID,PlaceID")] Order order)
        {
            if (ModelState.IsValid)
            {
                order.OrderDate = DateTime.Now;
                order.Price = 0;

                var table = await _context.Places.FindAsync(order.PlaceID);
                if (table != null && table.WaiterID != null)
                {
                    order.WaiterID = table.WaiterID;
                }

                _context.Add(order);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(order);
        }

        // ==========================================
        // 4. ОБРОБКА ТА ЗАВЕРШЕННЯ (СПИСАННЯ ПРОДУКТІВ)
        // ==========================================
        public async Task<IActionResult> ToActive(int? id)
        {
            if (id == null) return NotFound();
            var order = await _context.Orders.FirstOrDefaultAsync(m => m.ID == id);
            if (order == null) return NotFound();

            order.StatusID = _context.Statuses.First(x => x.Name.Equals("Активний")).ID;
            _context.Update(order);
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", new { id = order.ID });
        }

        public async Task<IActionResult> Finish(int? id)
        {
            if (id == null) return NotFound();

            // Завантажуємо замовлення
            var order = await _context.Orders.Include(o => o.OrderFoods).FirstOrDefaultAsync(m => m.ID == id);
            if (order == null) return NotFound();

            // Якщо офіціант не був призначений, призначаємо того, хто завершує замовлення
            if (order.WaiterID == null)
            {
                var currentUser = _context.Users.First(x => x.Login.Equals(HttpContext.User.Identity.Name));
                order.WaiterID = currentUser.ID;
            }

            // Змінюємо статус на "Завершений"
            order.StatusID = _context.Statuses.First(x => x.Name.Equals("Завершений")).ID;
            _context.Update(order);

            // МИ ВИДАЛИЛИ БЛОК З ЦИКЛОМ FOREACH, ЯКИЙ СПИСУВАВ ПРОДУКТИ (context.Products),
            // БО ЦІ МОДЕЛІ БІЛЬШЕ НЕ ІСНУЮТЬ У ПРОЕКТІ.

            await _context.SaveChangesAsync();
            return RedirectToAction("Details", new { id = order.ID });
        }

        // ==========================================
        // 5. ВИДАЛЕННЯ
        // ==========================================
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var order = await _context.Orders.Include(o => o.Client).Include(o => o.Status).FirstOrDefaultAsync(m => m.ID == id);
            if (order == null) return NotFound();
            return View(order);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order != null)
            {
                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}