using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Abstract;
using WebApp.Models;
using WebApp.Service;

namespace WebApp.Controllers
{
    public class AuthController : Controller
    {
        private readonly ILogger<AuthController> _logger;
        private readonly IUser _service;

        public AuthController(IUser service, ILogger<AuthController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        [AllowAnonymous] 
        public IActionResult Login() => View();

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register() => View();

        [HttpPost]
        [AllowAnonymous] 
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = _service.GetUserByEmail(model.Email);
            if (user == null || user.Password != model.Password)
            {
                ModelState.AddModelError("", "Invalid email or password");
                model.Password = string.Empty;
                return View(model);
            }
            else if (user.IsBlocked)
            {
                ModelState.AddModelError("", "User is blocked and cannot log in.");
                model.Password = string.Empty;
                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Email),
                new Claim("UserId", user.Id.ToString())
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(2)
            };

            user.LastSeen = DateTime.UtcNow;
            _service.UpdateLastSeen(user.Id, user.LastSeen.Value);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);
            return RedirectToAction("Index", "Admin");
        }

        [HttpPost]
        [AllowAnonymous] 
        [ValidateAntiForgeryToken]
        public IActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = new User
            {
                Name = model.FullName,
                Email = model.Email,
                Password = model.Password,
                IsBlocked = false,
                LastSeen = DateTime.UtcNow
            };

            try
            {
                _service.AddUser(user);
                TempData["RegistrationSuccess"] = "Registration successful! Please log in.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user registration");

                if (ex.Message.Contains("User with this email already exists"))
                {
                    TempData["LoginError"] = "User with this email already exists. Please log in instead.";
                    return RedirectToAction("Login");
                }

                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }
}