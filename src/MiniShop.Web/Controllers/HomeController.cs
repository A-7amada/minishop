using Microsoft.AspNetCore.Mvc;
using MiniShop.Application.Services;
namespace MiniShop.Web.Controllers;
public class HomeController(CatalogService catalog):Controller
{
 public async Task<IActionResult> Index(string? q,string? category,string? sort)
 {
  ViewBag.Query=q;ViewBag.Category=category;ViewBag.Sort=sort;
  var products=await catalog.SearchAsync(q);
  if(!string.IsNullOrEmpty(category))products=products.Where(p=>p.Category==category).ToList();
  products=sort switch{"low"=>products.OrderBy(p=>p.Price).ToList(),"high"=>products.OrderByDescending(p=>p.Price).ToList(),_=>products};
  return View(products);
 }
 public async Task<IActionResult> Details(int id){var p=await catalog.GetAsync(id);return p is null?NotFound():View(p);}
}
