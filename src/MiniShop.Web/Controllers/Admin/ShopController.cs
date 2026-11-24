using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniShop.Application.Services;
using MiniShop.Domain.Entities;
namespace MiniShop.Web.Controllers.Admin;
[Area("Admin"),Authorize(Policy="AdminOnly")]
public class ShopController(ShopService shop,ShippingService shipping):Controller
{
 public async Task<IActionResult> Coupons()=>View("~/Areas/Admin/Views/Shop/Coupons.cshtml",await shop.CouponsAsync());
 [HttpPost,ValidateAntiForgeryToken]
 public async Task<IActionResult> SaveCoupon([Bind("Id,Code,Percent,MinimumSpend,IsActive,ExpiresAt")] Coupon coupon)
 {
  coupon.Code=coupon.Code?.Trim().ToUpperInvariant()??"";
  ModelState.Clear();TryValidateModel(coupon);
  if(!ModelState.IsValid){TempData["Error"]="InvalidFields";return RedirectToAction("Coupons");}
  coupon.ExpiresAt=DateTime.SpecifyKind(coupon.ExpiresAt,DateTimeKind.Utc);
  try{await shop.SaveCouponAsync(coupon);}catch(InvalidOperationException){TempData["Error"]="DuplicateCode";}
  return RedirectToAction("Coupons");
 }
 public async Task<IActionResult> Orders()=>View("~/Areas/Admin/Views/Shop/Orders.cshtml",await shop.AllOrdersAsync());
 [HttpPost,ValidateAntiForgeryToken]
 public async Task<IActionResult> Status(int id,string status)
 {try{await shop.SetStatusAsync(id,status);}catch(InvalidOperationException){TempData["Error"]="InvalidStatus";}return RedirectToAction("Orders");}
 public async Task<IActionResult> Shipping()
 {ViewBag.Zones=await shipping.ZonesAsync();ViewBag.Regions=await shipping.RegionsAsync();ViewBag.Settings=await shipping.SettingsAsync();return View("~/Areas/Admin/Views/Shop/Shipping.cshtml");}
 [HttpPost,ValidateAntiForgeryToken]
 public async Task<IActionResult> SaveZone([Bind("Id,Name,NameAr,Fee,FreeThreshold,MinDays,MaxDays,IsActive")] ShippingZone zone)
 {
  ModelState.Clear();TryValidateModel(zone);
  if(!ModelState.IsValid){TempData["Error"]="InvalidFields";return RedirectToAction("Shipping");}
  try{await shipping.SaveZoneAsync(zone);TempData["Saved"]="Saved";}catch(InvalidOperationException e){TempData["Error"]=e.Message;}
  return RedirectToAction("Shipping");
 }
 [HttpPost,ValidateAntiForgeryToken]
 public async Task<IActionResult> SaveSettings([Bind("CodEnabled,CodFee,CodMaxOrder")] ShippingSettings settings)
 {
  ModelState.Clear();TryValidateModel(settings);
  if(!ModelState.IsValid){TempData["Error"]="InvalidFields";return RedirectToAction("Shipping");}
  await shipping.SaveSettingsAsync(settings);TempData["Saved"]="Saved";return RedirectToAction("Shipping");
 }
 [HttpPost,ValidateAntiForgeryToken]
 public async Task<IActionResult> SetRegion(int regionId,int zoneId){await shipping.SetRegionZoneAsync(regionId,zoneId);TempData["Saved"]="Saved";return RedirectToAction("Shipping");}
}
