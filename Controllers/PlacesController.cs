using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Restaurant.Data;
using Restaurant.Models;

namespace Restaurant.Controllers
{
    [Authorize(Roles = "Менеджер")] // Тільки менеджер може керувати столиками
    public class PlacesController : Controller
    {
        private readonly RestaurantContext _context;

        public PlacesController(RestaurantContext context)
        {
            _context = context;
        }

        // GET: Places
        public async Task<IActionResult> Index()
        {
            // Додаємо Include(p => p.Waiter), щоб бачити, хто закріплений за столом
            var places = await _context.Places.Include(p => p.Waiter).ToListAsync();
            return View(places);
        }

        // GET: Places/Create
        public IActionResult Create()
        {
            // Список офіціантів для вибору при створенні столика
            ViewData["WaiterID"] = new SelectList(_context.Users.Where(u => u.Role.Name == "Офіціант"), "ID", "FullName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Name,WaiterID")] Place place)
        {
            if (ModelState.IsValid)
            {
                _context.Add(place);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["WaiterID"] = new SelectList(_context.Users.Where(u => u.Role.Name == "Офіціант"), "ID", "FullName", place.WaiterID);
            return View(place);
        }

        // GET: Places/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var place = await _context.Places.FindAsync(id);
            if (place == null) return NotFound();

            // Передаємо список офіціантів у View для редагування
            ViewData["WaiterID"] = new SelectList(_context.Users.Where(u => u.Role.Name == "Офіціант"), "ID", "FullName", place.WaiterID);
            return View(place);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Name,WaiterID")] Place place)
        {
            if (id != place.ID) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(place);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PlaceExists(place.ID)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["WaiterID"] = new SelectList(_context.Users.Where(u => u.Role.Name == "Офіціант"), "ID", "FullName", place.WaiterID);
            return View(place);
        }

        // МЕТОД ДЛЯ ШВИДКОЇ ЗМІНИ ОФІЦІАНТА (Рокіровка)
        [HttpPost]
        public async Task<IActionResult> ReassignTable(int tableId, int waiterId)
        {
            var table = await _context.Places.FindAsync(tableId);
            if (table != null)
            {
                table.WaiterID = waiterId;
                await _context.SaveChangesAsync();
            }
            // Повертаємося на Index, де відображено список столиків
            return RedirectToAction(nameof(Index));
        }

        // GET: Places/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var place = await _context.Places.Include(p => p.Waiter).FirstOrDefaultAsync(m => m.ID == id);
            if (place == null) return NotFound();

            return View(place);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var place = await _context.Places.FindAsync(id);
            if (place != null)
            {
                _context.Places.Remove(place);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool PlaceExists(int id)
        {
            return _context.Places.Any(e => e.ID == id);
        }
    }
}