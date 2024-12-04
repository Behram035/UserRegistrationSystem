using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using UserRegistrationSystem.Models;
using UserRegistrationSystem.Services;

namespace UserRegistrationSystem.Controllers
{
    public class AuthController : Controller
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Register(UserDto userDto)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            var emailIsValid = new EmailAddressAttribute().IsValid(userDto.Email);
            if (!emailIsValid)
            {
                ViewData["RegisterError"] = "Invalid email format. Please enter a valid email address.";
                return View();
            }

            if (string.IsNullOrEmpty(userDto.Password) || !Regex.IsMatch(userDto.Password, @"^(?=.*[A-Z])(?=.*\d)[A-Za-z\d@$!%*?&]{8,}$"))
            {
                ViewData["RegisterError"] = "Password is too weak. It must be at least 8 characters long, contain at least one uppercase letter and one number.";
                return View();
            }

            var user = new User
            {
                Username = userDto.Username,
                Email = userDto.Email
            };

            var passwordRegistered = await _authService.RegisterAsync(user, userDto.Password);
            if (passwordRegistered)
            {
                return RedirectToAction("Login");
            }
            ViewData["RegisterError"] = "Email is already registered.";
            return View();
        }



        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = await _authService.AuthenticateAsync(email, password);
            if (user != null)
            {
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity(new[]
               {
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, user.Username),
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, user.Email)
                }, CookieAuthenticationDefaults.AuthenticationScheme)));

                return RedirectToAction("UserList");
            }
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            ViewData["LoginError"] = "Invalid email or password. Please try again.";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        [HttpGet]
        public async Task<IActionResult> UserList(int page = 1)
        {
            if (!User.Identity.IsAuthenticated) 
            {
                return RedirectToAction("Login");
            }

            int pageSize = 10;  
            var totalUsers = await _authService.GetUsersCountAsync();  
            var totalPages = (int)Math.Ceiling(totalUsers / (double)pageSize);
            var users = await _authService.GetUsersAsync(page, pageSize);

            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = totalPages;

            return View(users);
        }
    }
}
