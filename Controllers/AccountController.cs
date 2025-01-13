using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TaskManager.Models;

namespace TaskManager.Controllers
{
  public class AccountController : Controller
  {
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;

    public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
    {
      _userManager = userManager;
      _signInManager = signInManager;
    }

    // GET: /Account/Register
    public IActionResult Register()
    {
      if (User.Identity.IsAuthenticated)
      {
        return RedirectToAction("Task", "Home");
      }
      return View();
    }

    // POST: /Account/Register
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
      if (ModelState.IsValid)
      {
        var user = new IdentityUser { UserName = model.Username, Email = model.Email };
        var result = await _userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
          // Sign in the user after successful registration
          await _signInManager.SignInAsync(user, isPersistent: false);
          return RedirectToAction("Index", "Task");
        }
        else
        {
          foreach (var error in result.Errors)
          {
            ModelState.AddModelError(string.Empty, error.Description);
          }
        }
      }
      return View(model);
    }

    // GET: /Account/Login
    public IActionResult Login()
    {
      if (User.Identity.IsAuthenticated)
      {
        return RedirectToAction("Index", "Task");
      }
      return View();
    }

    // POST: /Account/Login
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
      if (ModelState.IsValid)
      {
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user != null)
        {
          var result = await _signInManager.PasswordSignInAsync(user, model.Password, isPersistent: false, lockoutOnFailure: true);
          if (result.Succeeded)
          {
            return RedirectToAction("Index", "Task");
          }
          else if (result.IsLockedOut)
          {
            ModelState.AddModelError(string.Empty, "Your account is locked.");
          }
          else
          {
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
          }
        }
        else
        {
          ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        }
      }
      return View(model);
    }

    // GET: /Account/Logout
    public async Task<IActionResult> Logout()
    {
      await _signInManager.SignOutAsync();
      return RedirectToAction("Index", "Home");
    }

    public async Task<IActionResult> Profile()
    {
      if (!User.Identity.IsAuthenticated)
      {
        return RedirectToAction("Login", "Account");
      }

      var user = await _userManager.GetUserAsync(User);
      if (user == null)
      {
        return RedirectToAction("Login", "Account");
      }

      var model = new ProfileViewModel
      {
        Email = user.Email,
        Username = user.UserName
      };

      return View(model);
    }
  }
}
