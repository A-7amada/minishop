using Microsoft.AspNetCore.Mvc;
using MiniShop.Application.Services;

namespace MiniShop.Web.Controllers;

public class CartController : Controller
{
    private readonly CatalogService _catalog;
    private readonly CartService _cart;
    public CartController(CatalogService catalog, CartService cart) { _catalog = catalog; _cart = cart; }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(int id,int quantity){var p=await _catalog.GetAsync(id);if(p is not null)_cart.Update(id,quantity,p.Stock);return RedirectToAction("Index");}
    public IActionResult Index() => View(_cart.Get());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(int id)
    {
        var product = await _catalog.GetAsync(id);
        if (product is not null && product.Stock > _cart.Get().Where(l=>l.ProductId==id).Sum(l=>l.Quantity)) _cart.Add(product);
        return RedirectToAction("Index");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Remove(int id)
    {
        _cart.Remove(id);
        return RedirectToAction("Index");
    }
}
