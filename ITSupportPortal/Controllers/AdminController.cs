using ITSupportPortal.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ITSupportPortal.Controllers
{
    public class AdminController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<AdminController> _logger;

        public AdminController(ILogger<AdminController> logger ,UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, RoleManager<IdentityRole> roleManager)
        {
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateUser()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles="Admin")]
        public async Task<IActionResult> CreateUser(AdminRegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var exists = await _userManager.FindByEmailAsync(model.Email);
                if (exists == null)
                {
                    var user = new IdentityUser(model.Username)
                    {
                        Email = model.Email,
                    };
                    var result = await _userManager.CreateAsync(user, model.Password);
                    if (result.Succeeded)
                    {
                        //Create role if doesn't exist
                        var roleExist = await _roleManager.RoleExistsAsync(model.accountType.ToString());
                        if (!roleExist)
                        {
                            var roleResult = await _roleManager.CreateAsync(new IdentityRole(model.accountType.ToString()));
                            _logger.LogInformation("Role {role} created. ", model.accountType.ToString());

                        }
                        var role_assign_result = await _userManager.AddToRoleAsync(user, model.accountType.ToString());

                        //Check if user was created and role was assigned
                        if (result.Succeeded && role_assign_result.Succeeded)
                        {
                            _logger.LogInformation("User {user} created. ", model.Username);
                            return View("UserCreated");
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
    }
}
