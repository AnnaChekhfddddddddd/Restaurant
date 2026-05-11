using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Restaurant.Data;
using Restaurant.Models;

namespace Restaurant.Controllers
{
    public class OrderFoodsController : Controller
    {
        private readonly RestaurantContext _context;

        public OrderFoodsController(RestaurantContext context)
        {
            _context = context;
        }

        // GET: OrderFoods
        public async Task<IActionResult> Index()
        {
            var dietFoodContext = _context.OrderFoods.Include(o => o.Food).Include(o => o.Order);
            return View(await dietFoodContext.ToListAsync());
        }

        // GET: OrderFoods/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var orderFood = await _context.OrderFoods
                .Include(o => o.Food)
                .Include(o => o.Order)
                .FirstOrDefaultAsync(m => m.ID == id);

            if (orderFood == null) return NotFound();

            return View(orderFood);
        }

        // GET: OrderFoods/Create
        public IActionResult Create(int? id)
        {
            ViewData["FoodID"] = new SelectList(_context.Foods, "ID", "Name");
            // Передаємо ID замовлення, щоб автоматично прив'язати страву
            ViewData["OrderID"] = new SelectList(_context.Orders, "ID", "ID", id);
            ViewBag.OrderID = id;
            return View();
        }

        // POST: OrderFoods/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("OrderID,FoodID,Quantity")] OrderFood orderFood)
        {
            if (ModelState.IsValid)
            {
                _context.Add(orderFood);
                await _context.SaveChangesAsync();

                // Оновлюємо ціну замовлення
                var food = await _context.Foods.FindAsync(orderFood.FoodID);
                var order = await _context.Orders.FindAsync(orderFood.OrderID);

                if (food != null && order != null)
                {
                    order.Price += food.Price * orderFood.Quantity;
                    _context.Orders.Update(order);
                    await _context.SaveChangesAsync();
                }

                return Redirect("~/Orders/Details/" + orderFood.OrderID);
            }
            ViewData["FoodID"] = new SelectList(_context.Foods, "ID", "Name", orderFood.FoodID);
            ViewData["OrderID"] = new SelectList(_context.Orders, "ID", "ID", orderFood.OrderID);
            return View(orderFood);
        }

        // GET: OrderFoods/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var orderFood = await _context.OrderFoods.FindAsync(id);
            if (orderFood == null) return NotFound();

            ViewData["FoodID"] = new SelectList(_context.Foods, "ID", "Name", orderFood.FoodID);
            ViewData["OrderID"] = new SelectList(_context.Orders, "ID", "ID", orderFood.OrderID);
            return View(orderFood);
        }

        // POST: OrderFoods/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,OrderID,FoodID,Quantity")] OrderFood orderFood)
        {
            if (id != orderFood.ID) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // Важливо: перерахунок ціни при редагуванні кількості зазвичай складніший,
                    // але для простоти ми просто оновлюємо запис.
                    _context.Update(orderFood);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderFoodExists(orderFood.ID)) return NotFound();
                    else throw;
                }
                return Redirect("~/Orders/Details/" + orderFood.OrderID);
            }
            ViewData["FoodID"] = new SelectList(_context.Foods, "ID", "Name", orderFood.FoodID);
            ViewData["OrderID"] = new SelectList(_context.Orders, "ID", "ID", orderFood.OrderID);
            return View(orderFood);
        }

        // GET: OrderFoods/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var orderFood = await _context.OrderFoods
                .Include(o => o.Food)
                .Include(o => o.Order)
                .FirstOrDefaultAsync(m => m.ID == id);

            if (orderFood == null) return NotFound();

            return View(orderFood);
        }

        // POST: OrderFoods/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var orderFood = await _context.OrderFoods
                .Include(o => o.Food)
                .Include(o => o.Order)
                .FirstOrDefaultAsync(x => x.ID == id);

            if (orderFood != null)
            {
                // Віднімаємо ціну видаленої страви від загального чека
                var order = orderFood.Order;
                var food = orderFood.Food;

                order.Price -= food.Price * orderFood.Quantity;

                _context.OrderFoods.Remove(orderFood);
                _context.Orders.Update(order);

                await _context.SaveChangesAsync();
                return Redirect("~/Orders/Details/" + orderFood.OrderID);
            }

            return RedirectToAction("Index", "Orders");
        }

        private bool OrderFoodExists(int id)
        {
            return _context.OrderFoods.Any(e => e.ID == id);
        }
    }
}