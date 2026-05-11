using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Restaurant.Data;
using Restaurant.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Restaurant.Controllers
{
    public class AccountController : Controller
    {
        private readonly RestaurantContext db;
        private readonly IHostingEnvironment Environment;

        public AccountController(RestaurantContext context, IHostingEnvironment _environment)
        {
            db = context;
            Environment = _environment;
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                User user = await db.Users.Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.Login == model.Login && u.Password == model.Password);

                if (user != null)
                {
                    HttpContext.Session.Clear();
                    await Authenticate(user);
                    return RedirectToAction("Index", "Home");
                }
                ModelState.AddModelError("", "Неправильний логін або пароль");
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            ViewBag.Roles = new SelectList(db.Roles.Where(x => x.Name == "Клієнт"), "ID", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterModel model, IFormFile file)
        {
            if (ModelState.IsValid)
            {
                User user = await db.Users.FirstOrDefaultAsync(u => u.Login == model.Login);
                if (user == null)
                {
                    var clientRole = await db.Roles.FirstOrDefaultAsync(r => r.Name == "Клієнт");
                    int finalRoleId = clientRole != null ? clientRole.ID : 0;

                    User registerUser = new User
                    {
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        Login = model.Login,
                        Password = model.Password,
                        RoleID = finalRoleId,
                        Avatar = "/img/client_avatar.png"
                    };

                    db.Users.Add(registerUser);
                    await db.SaveChangesAsync();

                    if (file != null)
                    {
                        string path = Path.Combine(this.Environment.WebRootPath, "Images/Users/" + registerUser.ID);
                        if (!Directory.Exists(path)) Directory.CreateDirectory(path);

                        string fileName = Path.GetFileName(file.FileName);
                        using (var stream = new FileStream(Path.Combine(path, fileName), FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        registerUser.Avatar = "/Images/Users/" + registerUser.ID + "/" + fileName;
                        db.Users.Update(registerUser);
                        await db.SaveChangesAsync();
                    }

                    HttpContext.Session.Clear();
                    await Authenticate(registerUser);
                    return RedirectToAction("Index", "Home");
                }
                ModelState.AddModelError("", "Цей логін вже зайнятий");
            }
            return View(model);
        }

        // МЕТОД ПЕРЕГЛЯДУ ПРОФІЛЮ
        public async Task<IActionResult> Profile()
        {
            var userName = User.Identity.Name;
            var user = await db.Users.Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Login == userName);

            if (user == null) return RedirectToAction("Login");

            return View(user);
        }

        // МЕТОД РЕДАГУВАННЯ (GET)
        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var userName = User.Identity.Name;
            var user = await db.Users.FirstOrDefaultAsync(u => u.Login == userName);

            if (user == null) return RedirectToAction("Login");

            // Вказуємо точний шлях до файлу, щоб уникнути помилки "View not found"
            return View("~/Views/Account/Edit.cshtml", user);
        }

        // МЕТОД ЗБЕРЕЖЕННЯ (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(User model, IFormFile file)
        {
            var user = await db.Users.FirstOrDefaultAsync(u => u.ID == model.ID);
            if (user == null) return NotFound();

            // Перевіряємо чи змінився логін
            bool loginChanged = user.Login != model.Login;

            // Оновлюємо поля
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Phone = model.Phone;
            user.Login = model.Login;
            user.Password = model.Password;

            if (file != null)
            {
                string path = Path.Combine(this.Environment.WebRootPath, "Images/Users/" + user.ID);
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);

                string fileName = Path.GetFileName(file.FileName);
                using (var stream = new FileStream(Path.Combine(path, fileName), FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                user.Avatar = "/Images/Users/" + user.ID + "/" + fileName;
            }

            db.Users.Update(user);
            await db.SaveChangesAsync();

            // Якщо логін змінився, оновлюємо сесію, щоб не викидало
            if (loginChanged)
            {
                await Authenticate(user);
            }

            return RedirectToAction("Profile");
        }

        private async Task Authenticate(User user)
        {
            var userWithRole = await db.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.ID == user.ID);
            string roleName = userWithRole?.Role?.Name ?? "Клієнт";

            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, user.Login),
                new Claim(ClaimsIdentity.DefaultRoleClaimType, roleName)
            };

            ClaimsIdentity id = new ClaimsIdentity(claims, "ApplicationCookie",
                ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);

            // Оновлюємо куки
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
        }

        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }
    }
}