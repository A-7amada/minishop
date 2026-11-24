using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniShop.Application.Services;
namespace MiniShop.Web.Controllers;
[Authorize]
public class WishlistController(ShopService shop,CatalogService catalog):Controller
{
 public async Task<IActionResult> Index(){var ids=await shop.WishlistAsync(User.Identity!.Name!);return View((await catalog.SearchAsync(null)).Where(p=>ids.Contains(p.Id)).ToList());}
 [HttpPost,ValidateAntiForgeryToken]
 public async Task<IActionResult> Toggle(int id,string? returnUrl){await shop.ToggleWishAsync(User.Identity!.Name!,id);return LocalRedirect(Url.IsLocalUrl(returnUrl)?returnUrl!:"/Wishlist");}
}
