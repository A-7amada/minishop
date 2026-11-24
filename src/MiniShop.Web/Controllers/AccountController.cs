using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniShop.Application.Services;
using MiniShop.Domain.Entities;

namespace MiniShop.Web.Controllers;

public class AccountController : Controller
{
    private readonly AccountService _accounts;
    private readonly OrderService _orders;
    public AccountController(AccountService accounts, OrderService orders) { _accounts = accounts; _orders = orders; }

    [HttpGet]
    public IActionResult Register() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(string fullName, string email, string password)
    {
        var (ok, error, user) = await _accounts.RegisterAsync(fullName, email, password);
        if (!ok || user is null)
        {
            ViewBag.Error = error;
            return View();
        }
        await SignIn(user);
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string email, string password)
    {
        var user = await _accounts.ValidateLoginAsync(email, password);
        if (user is null)
        {
            ViewBag.Error = "Invalid email or password.";
            return View();
        }
        await SignIn(user);
        return RedirectToAction("Index", "Home");
    }

    [Authorize]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    [Authorize]
    public async Task<IActionResult> Orders() => View(await _orders.ListForUserAsync(User.Identity!.Name!));

    [Authorize, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        if (!await _orders.CancelAsync(User.Identity!.Name!, id)) TempData["Error"] = "CannotCancel";
        return RedirectToAction("Orders");
    }

    private async Task SignIn(AppUser user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, user.Email),
            new("display_name", user.FullName),
            new("is_admin", user.IsAdmin ? "true" : "false")
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(new ClaimsPrincipal(identity));
    }
}
