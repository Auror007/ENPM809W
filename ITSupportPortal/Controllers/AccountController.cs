using ITSupportPortal.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.DependencyInjection;

namespace ITSupportPortal.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<AccountController> _logger;
        public AccountController(ILogger<AccountController> logger,UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, RoleManager<IdentityRole> roleManager)
        {
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl)
        {
            return View(new LoginViewModel
            {
                ReturnUrl = returnUrl,
            });
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(model.Username, model.Password, false , lockoutOnFailure: true);

            if (result.Succeeded)
            {
                _logger.LogInformation("{user} logged in to the application",model.Username);
                if (model.ReturnUrl != null)
                {
                    return Redirect(model.ReturnUrl);
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }

            if (result.IsLockedOut)
            {
                _logger.LogInformation("{user} is locked out", model.Username);
                return View("Lockout");
            }
            else
            {
                _logger.LogError("{user} login failed.", model.Username);
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View(model);
            }
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            HttpContext.Session.Clear();
            foreach (var cookie in Request.Cookies.Keys)
            {
                Response.Cookies.Delete(cookie);
            }
            _logger.LogInformation("{user} logged out", User.Identity.Name);
            return RedirectToAction("Index", "Home");
        }


        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Register()
        {
            await _signInManager.SignOutAsync();
            return View(new RegisterViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var exists = await _userManager.FindByEmailAsync(model.Email);
                if (exists == null)
                {
                    var user = new IdentityUser(model.Username)
                    {
                        Email = model.Email
                    };
                    var result = await _userManager.CreateAsync(user, model.Password);
                    if (result.Succeeded)
                    {
                        //Create role if doesn't exist
                        var roleExist = await _roleManager.RoleExistsAsync("Customer");
                        if (!roleExist)
                        {
                            var roleResult = await _roleManager.CreateAsync(new IdentityRole("Customer"));
                            _logger.LogInformation("Customer Role created. ");

                        }
                        var role_assign_result = await _userManager.AddToRoleAsync(user, "Customer");

                        //Check if user was created and role was assigned
                        if (result.Succeeded && role_assign_result.Succeeded)
                        {
                            _logger.LogInformation("User {user} created. ", model.Username);
                            await _signInManager.SignInAsync(user, isPersistent: false);
                            return RedirectToAction("Index", "Home");
                        }
                    }
                   
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Email Already Exists.");
                    _logger.LogWarning(" Attempted to create user with same email: {email} ", model.Email);
                    return View(model); 
                }
               
            }

            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View(new LoginViewModel
            {
                ReturnUrl = "/Account/Login",
            });
        }


    }
}
