using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniShop.Application.DTOs;
using MiniShop.Application.Services;
namespace MiniShop.Web.Controllers;
[Authorize]
public class CheckoutController(OrderService orders, CartService cart, ShopService shop, CatalogService catalog, ShippingService shipping) : Controller
{
 const string DraftKey="checkout-draft";
 CheckoutDraft? LoadDraft(){var raw=HttpContext.Session.GetString(DraftKey);return raw is null?null:JsonSerializer.Deserialize<CheckoutDraft>(raw);}
 void SaveDraft(CheckoutDraft d)=>HttpContext.Session.SetString(DraftKey,JsonSerializer.Serialize(d));
 async Task Reprice(List<CartLine> lines){foreach(var line in lines){var p=await catalog.GetAsync(line.ProductId);if(p is not null)line.Price=p.Price;}}
 [HttpGet]
 public async Task<IActionResult> Index()
 {
  var lines=cart.Get();if(lines.Count==0)return RedirectToAction("Index","Cart");
  await Reprice(lines);
  var draft=LoadDraft();
  if(draft is null)
  {
   // Prefill from the customer's most recent delivery address.
   var last=await shipping.LastOrderAsync(User.Identity!.Name!);var regions0=await shipping.RegionsAsync();
   var rid=last is null?0:regions0.FirstOrDefault(r=>r.Name==last.Governorate)?.Id??0;
   draft=last is null?new CheckoutDraft("","",0,"","",ShippingService.Mock):new CheckoutDraft(last.FullName,last.Phone,rid,last.City,last.AddressLine,ShippingService.Mock);
  }
  var code=HttpContext.Session.GetString("coupon");
  var subtotal=lines.Sum(l=>l.Subtotal);var result=await shop.DiscountAsync(code,subtotal);
  var basket=subtotal-result.discount;
  var quote=await shipping.QuoteAsync(draft.RegionId,basket,draft.Payment);
  var settings=await shipping.SettingsAsync();
  // When the chosen option is unusable (e.g. COD over the cap) price without COD so the page still shows a fee.
  var shown=quote.Error=="CodLimit"||quote.Error=="CodUnavailable"?await shipping.QuoteAsync(draft.RegionId,basket,ShippingService.Mock):quote;
  ViewBag.Coupon=code;ViewBag.Discount=result.discount;ViewBag.Draft=draft;ViewBag.Regions=await shipping.RegionsAsync();ViewBag.Quote=shown;ViewBag.Settings=settings;
  ViewBag.Total=basket+(shown.Ok||shown.Fee>0||shown.Free?shown.Fee:0)+(quote.Ok?quote.CodFee:0);
  ViewBag.Error=TempData["Error"]??result.error??(draft.RegionId!=0?quote.Error:null);
  ViewBag.CodBlocked=quote.Error is "CodLimit" or "CodUnavailable";
  return View(lines);
 }
 // Saves the address / payment choice (called when the governorate or payment option changes) without losing typed fields.
 [HttpPost,ValidateAntiForgeryToken]
 public IActionResult Draft(string? fullName,string? phone,int regionId,string? city,string? line,string? payment)
 {SaveDraft(new CheckoutDraft(fullName,phone,regionId,city,line,payment==ShippingService.Cod?ShippingService.Cod:ShippingService.Mock));return RedirectToAction("Index");}
 [HttpPost,ValidateAntiForgeryToken]
 public async Task<IActionResult> Coupon(string? code,string? fullName,string? phone,int regionId,string? city,string? line,string? payment,string? clear)
 {
  if(fullName is not null||phone is not null||city is not null||line is not null||regionId!=0)SaveDraft(new CheckoutDraft(fullName,phone,regionId,city,line,payment==ShippingService.Cod?ShippingService.Cod:ShippingService.Mock));
  if(clear is not null){HttpContext.Session.Remove("coupon");return RedirectToAction("Index");}
  var lines=cart.Get();await Reprice(lines);
  var result=await shop.DiscountAsync(code,lines.Sum(l=>l.Subtotal));
  if(result.error is null)HttpContext.Session.SetString("coupon",code?.Trim().ToUpperInvariant()??"");
  else {HttpContext.Session.Remove("coupon");TempData["Error"]=result.error;}
  return RedirectToAction("Index");
 }
 [HttpPost,ValidateAntiForgeryToken]
 public async Task<IActionResult> Pay(string? fullName,string? phone,int regionId,string? city,string? line,string? payment)
 {
  var lines=cart.Get();if(lines.Count==0)return RedirectToAction("Index","Cart");
  var draft=new CheckoutDraft(fullName,phone,regionId,city,line,payment==ShippingService.Cod?ShippingService.Cod:ShippingService.Mock);
  SaveDraft(draft);
  try {
   var order=await orders.CreateFromCartAsync(User.Identity!.Name!,lines,HttpContext.Session.GetString("coupon"),draft);
   cart.Clear();HttpContext.Session.Remove("coupon");HttpContext.Session.Remove(DraftKey);return View("Confirmation",order);
  } catch(InvalidOperationException e){TempData["Error"]=e.Message;return RedirectToAction("Index");}
 }
}
