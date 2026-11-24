using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniShop.Application.Services;
using MiniShop.Domain.Entities;

namespace MiniShop.Web.Controllers.Admin;

[Area("Admin")]
[Authorize(Policy = "AdminOnly")]
public class ProductsController : Controller
{
    private readonly CatalogService _catalog;
    public ProductsController(CatalogService catalog) => _catalog = catalog;

    public async Task<IActionResult> Index() => View(await _catalog.SearchAsync(null));

    [HttpGet]
    public IActionResult Create() => View(new Product());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Product product)
    {
        if (!ModelState.IsValid) return View(product);
        await _catalog.CreateAsync(product);
        return RedirectToAction("Index");
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var product = await _catalog.GetAsync(id);
        return product is null ? NotFound() : View(product);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Product product)
    {
        if (!ModelState.IsValid) return View(product);
        await _catalog.UpdateAsync(product);
        return RedirectToAction("Index");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _catalog.DeleteAsync(id);
        return RedirectToAction("Index");
    }
}
