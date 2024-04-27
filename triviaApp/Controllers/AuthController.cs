using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using triviaApp.Models;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace triviaApp.Controllers
{
    public class AuthController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AuthController(UserManager<User> userManager, SignInManager<User> signInManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        // GET: api/Category
        [HttpGet]
        public async Task<IActionResult> List()
        {
            var users = await _userManager.Users.Select(z=> new UserViewModel()
            {
                Id = z.Id,
                UserName = z.UserName
            }).ToListAsync();

            return View(users);
        }

        // GET: api/Category/5
        [HttpGet("Auth/Details/{id}")]
        public async Task<IActionResult> Details(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return RedirectToAction("List", "Category");
            }

            return View(new EditUserViewModel()
            {
                Id = user.Id,
                UserName = user.UserName
            });
        }

        [Authorize]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new User { UserName = model.Username};
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    if (!_roleManager.RoleExistsAsync("Admin").Result)
                    {
                        await _roleManager.CreateAsync(new IdentityRole("Admin"));
                    }

                    await _userManager.AddToRoleAsync(user, "Admin");
                  
                    return RedirectToAction("Index", "Admin");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return View(model);
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Username, model.Password, false, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Admin");
                }
                ModelState.AddModelError("", "Giriş işlemi başarısız, kullanıcı adınızı ya da şifrenizi kontrol ediniz.");
            }
            return View(model);
        }

        // POST: api/Category
        [HttpPost]
        public async Task<IActionResult> Edit(EditUserViewModel user)
        {
            var userEntity = await _userManager.FindByIdAsync(user.Id);

            if (!string.IsNullOrEmpty(user.UserName))
            {
                userEntity.UserName = user.UserName;
            }

            if (!string.IsNullOrEmpty(user.Password) && user.Password == user.ConfirmPassword)
            {
                 await _userManager.ChangePasswordAsync(userEntity, user.OldPassword, user.Password);
            }

            var result = await _userManager.UpdateAsync(userEntity);
            
            if (result.Succeeded)
            {
                return RedirectToAction("List");
            }

            return RedirectToAction("List", "Auth");
        }

        [HttpDelete]
        public async Task<IActionResult> Remove(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                return RedirectToAction("List");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return RedirectToAction("List");
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Auth");
        }
    }
}

