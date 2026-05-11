using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Restaurant.Data;
using Restaurant.Models;
using Microsoft.AspNetCore.Authorization;

namespace Restaurant.Controllers
{
    public class FoodsController : Controller
    {
        private readonly RestaurantContext _context;
        private readonly IWebHostEnvironment _environment;

        public FoodsController(RestaurantContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: Foods
        public async Task<IActionResult> Index(string? FoodName, int? CategoryID, int? Price)
        {
            // 1. Створюємо список з вашим пріоритетним порядком категорій
            var categoryOrder = new List<string>
            {
                "Перші страви",
                "Основні страви",
                "Гарячі закуски",
                "Гарніри",
                "Випічка",
                "Десерти",
                "Безалкогольні/Алкогольні напої"
            };

            var query = _context.Foods.Include(f => f.FoodType).AsQueryable();

            // Фільтрація
            if (!string.IsNullOrEmpty(FoodName))
                query = query.Where(x => x.Name.ToLower().Contains(FoodName.ToLower()));

            if (CategoryID != null)
                query = query.Where(x => x.FoodTypeID == CategoryID);

            if (Price != null)
                query = query.Where(x => x.Price == Price);

            // 2. Отримуємо дані з бази
            var foodsList = await query.ToListAsync();

            // 3. Сортуємо страви згідно зі списком categoryOrder
            var orderedFoods = foodsList
                .OrderBy(f => {
                    var index = categoryOrder.IndexOf(f.FoodType.Name);
                    return index == -1 ? 99 : index; // Невідомі категорії йдуть в кінець
                })
                .ThenBy(f => f.Name)
                .ToList();

            // 4. Сортуємо категорії для фільтра (випадаючий список), щоб вони теж йшли по порядку
            var allCategories = await _context.FoodTypes.ToListAsync();
            var sortedCategoriesForFilter = allCategories
                .OrderBy(c => {
                    var index = categoryOrder.IndexOf(c.Name);
                    return index == -1 ? 99 : index;
                })
                .ToList();

            ViewData["AllCategories"] = new SelectList(sortedCategoriesForFilter, "ID", "Name");
            ViewData["CategoryID"] = CategoryID;

            return View(orderedFoods);
        }

        // POST: Foods/ToggleStatus/5
        [Authorize(Roles = "Менеджер")]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var food = await _context.Foods.FindAsync(id);
            if (food != null)
            {
                food.IsAvailable = !food.IsAvailable;
                _context.Update(food);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Foods/DeleteConfirmedQuickly/5
        [Authorize(Roles = "Менеджер")]
        public async Task<IActionResult> DeleteConfirmedQuickly(int id)
        {
            var food = await _context.Foods.FindAsync(id);
            if (food != null)
            {
                _context.Foods.Remove(food);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Foods/Create
        [Authorize(Roles = "Менеджер")]
        public IActionResult Create()
        {
            ViewData["FoodTypeID"] = new SelectList(_context.FoodTypes, "ID", "Name");
            return View();
        }

        // POST: Foods/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Менеджер")]
        public async Task<IActionResult> Create([Bind("Name,FoodTypeID,Price")] Food food, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                food.Image = "/img/food.png";
                food.IsAvailable = true;
                _context.Add(food);
                await _context.SaveChangesAsync();

                if (file != null)
                {
                    string folderPath = Path.Combine(_environment.WebRootPath, "Images/Foods", food.ID.ToString());
                    if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                    string fileName = Path.GetFileName(file.FileName);
                    string fullPath = Path.Combine(folderPath, fileName);

                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    food.Image = $"/Images/Foods/{food.ID}/{fileName}";
                    _context.Update(food);
                    await _context.SaveChangesAsync();
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["FoodTypeID"] = new SelectList(_context.FoodTypes, "ID", "Name", food.FoodTypeID);
            return View(food);
        }

        // GET: Foods/Edit/5
        [Authorize(Roles = "Менеджер")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var food = await _context.Foods.FindAsync(id);
            if (food == null) return NotFound();

            ViewData["FoodTypeID"] = new SelectList(_context.FoodTypes, "ID", "Name", food.FoodTypeID);
            return View(food);
        }

        // POST: Foods/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Менеджер")]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Name,FoodTypeID,Price,Image,IsAvailable")] Food food, IFormFile? imageFile)
        {
            if (id != food.ID) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    if (imageFile != null)
                    {
                        string folderPath = Path.Combine(_environment.WebRootPath, "Images/Foods", food.ID.ToString());
                        if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                        string fileName = Path.GetFileName(imageFile.FileName);
                        string fullPath = Path.Combine(folderPath, fileName);

                        using (var stream = new FileStream(fullPath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(stream);
                        }

                        food.Image = $"/Images/Foods/{food.ID}/{fileName}";
                    }
                    else
                    {
                        var existingFood = await _context.Foods.AsNoTracking().FirstOrDefaultAsync(f => f.ID == id);
                        if (existingFood != null)
                        {
                            food.Image = existingFood.Image;
                        }
                    }

                    _context.Update(food);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FoodExists(food.ID)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["FoodTypeID"] = new SelectList(_context.FoodTypes, "ID", "Name", food.FoodTypeID);
            return View(food);
        }

        private bool FoodExists(int id)
        {
            return _context.Foods.Any(e => e.ID == id);
        }
    }
}