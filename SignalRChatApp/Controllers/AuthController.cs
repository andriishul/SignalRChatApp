using SignalRChatApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;

namespace SignalRChatApp.Controllers
{
    public class AuthController : Controller
    {
        private readonly ILogger<AuthController> logger;
        private readonly UserManager<IdentityUser> userManager;
        private readonly SignInManager<IdentityUser> signInManager;
        public AuthController(SignInManager<IdentityUser> signInManager, 
            UserManager<IdentityUser> userManager, ILogger<AuthController> logger) 
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
            this.logger = logger;
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model) 
        {
            if (!ModelState.IsValid)
                return View(model);

            var identity = new IdentityUser 
            {
                Email = model.Email,
                UserName = model.Username
            };

            var existingUser = await userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "An account with this email already exists.");
                return View(model);
            }

            IdentityResult identityResult = await userManager.CreateAsync(identity, model.Password);
            
            if (!identityResult.Succeeded)
            {
                foreach (var error in identityResult.Errors)
                {
                    logger.LogError(error.Description);
                }
            }
            
            await signInManager.SignInAsync(identity, false);
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult SignIn() => View();

        [HttpPost]
        public async Task<IActionResult> SignIn(SignInViewModel model) 
        {
            var user = await userManager.FindByEmailAsync(model.Email);
            
            if (user == null) 
            {
                ModelState.AddModelError("Email", "Email is incorrect.");
                return View(model);
            }

            var result = await signInManager.PasswordSignInAsync(user, model.Password, false, false);
            
            if (!result.Succeeded)
            {
                ModelState.AddModelError("Password", "Password is incorrect.");
                return View(model);
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> LogOut() 
        {
            await signInManager.SignOutAsync();

            return RedirectToAction("Index", "Home");
        }
    }
}
