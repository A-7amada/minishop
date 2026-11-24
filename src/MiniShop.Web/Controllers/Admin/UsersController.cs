using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniShop.Application.Services;
namespace MiniShop.Web.Controllers.Admin;
[Area("Admin"),Authorize(Policy="AdminOnly")]
public class UsersController:Controller
{
 private readonly AccountService _accounts; public UsersController(AccountService accounts){_accounts=accounts;}
 public async Task<IActionResult> Index()=>View("~/Areas/Admin/Views/Users/Index.cshtml",await _accounts.ListUsersAsync());
 [HttpPost,ValidateAntiForgeryToken]
 public async Task<IActionResult> Toggle(int id)
 {if(!await _accounts.SetActiveAsync(id,!string.Equals(Request.Form["active"],"true")))TempData["Error"]="CannotChangeAdmin";return RedirectToAction("Index");}
 [HttpPost,ValidateAntiForgeryToken]
 public async Task<IActionResult> Delete(int id)
 {if(!await _accounts.DeleteUserAsync(id))TempData["Error"]="CannotChangeAdmin";return RedirectToAction("Index");}
}
