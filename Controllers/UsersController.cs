using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Restaurant.Data;
using Restaurant.Models;

namespace Restaurant.Controllers
{
    public class UsersController : Controller
    {
        private readonly RestaurantContext _context;
        private readonly IWebHostEnvironment Environment;

        public UsersController(RestaurantContext context, IWebHostEnvironment _environment)
        {
            _context = context;
            Environment = _environment;
        }

        // --- ГОЛОВНА СТОРІНКА ДЛЯ МЕНЕДЖЕРА (ТІЛЬКИ ПЕРСОНАЛ) ---
        [Authorize(Roles = "Менеджер")]
        public async Task<IActionResult> Staff()
        {
            var staff = await _context.Users
                .Include(u => u.Role)
                .Where(u => u.Role.Name != "Клієнт") // Показуємо всіх, крім клієнтів
                .ToListAsync();

            ViewData["Title"] = "Персонал ресторану";
            return View(staff);
        }

        // ПРОФІЛЬ ПОТОЧНОГО КОРИСТУВАЧА
        [Authorize]
        public async Task<IActionResult> My()
        {
            var login = User.Identity.Name;
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(m => m.Login == login);

            if (user == null) return NotFound();
            ViewBag.My = true;
            return View("Details", user);
        }

        // --- CRUD ОПЕРАЦІЇ ---

        [Authorize(Roles = "Менеджер")]
        public IActionResult Create()
        {
            ViewData["RoleID"] = new SelectList(_context.Roles.Where(r => r.Name != "Клієнт"), "ID", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Менеджер")]
        public async Task<IActionResult> Create([Bind("ID,Login,Password,FirstName,LastName,Phone,RoleID")] User user, IFormFile file)
        {
            if (ModelState.IsValid)
            {
                _context.Add(user);
                await _context.SaveChangesAsync();

                if (file != null)
                {
                    await SaveUserImage(user, file);
                }

                // ТУТ ЗМІНА: Повертаємо на Staff
                return RedirectToAction(nameof(Staff));
            }
            ViewData["RoleID"] = new SelectList(_context.Roles, "ID", "Name", user.RoleID);
            return View(user);
        }

        [Authorize(Roles = "Менеджер")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();
            ViewData["RoleID"] = new SelectList(_context.Roles.Where(r => r.Name != "Клієнт"), "ID", "Name", user.RoleID);
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Менеджер")]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Login,Password,FirstName,LastName,Phone,Avatar,RoleID")] User user, IFormFile file)
        {
            if (id != user.ID) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    if (file != null)
                    {
                        await SaveUserImage(user, file);
                    }
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.ID)) return NotFound();
                    else throw;
                }
                // ТУТ ЗМІНА: Повертаємо на Staff
                return RedirectToAction(nameof(Staff));
            }
            ViewData["RoleID"] = new SelectList(_context.Roles, "ID", "Name", user.RoleID);
            return View(user);
        }

        [Authorize(Roles = "Менеджер")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var user = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(m => m.ID == id);
            if (user == null) return NotFound();
            return View(user);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Менеджер")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
            // ТУТ ЗМІНА: Повертаємо на Staff
            return RedirectToAction(nameof(Staff));
        }

        // --- ДОПОМІЖНІ МЕТОДИ ---
        private async Task SaveUserImage(User user, IFormFile file)
        {
            string folderPath = Path.Combine(Environment.WebRootPath, "Images/Users", user.ID.ToString());
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            string fileName = Path.GetFileName(file.FileName);
            string fullPath = Path.Combine(folderPath, fileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            user.Avatar = $"/Images/Users/{user.ID}/{fileName}";
            _context.Update(user);
            await _context.SaveChangesAsync();
        }

        private bool UserExists(int id) => _context.Users.Any(e => e.ID == id);
    }
}