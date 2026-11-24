using Microsoft.EntityFrameworkCore;
using MiniShop.Domain.Entities;
using MiniShop.Domain.Interfaces;
using MiniShop.Infrastructure.Persistence;
namespace MiniShop.Infrastructure.Repositories;
public class ShopRepository(AppDbContext db) : IShopRepository
{
 public Task<List<Coupon>> CouponsAsync() => db.Coupons.OrderBy(c=>c.Code).ToListAsync();
 public Task<Coupon?> CouponAsync(string code) => db.Coupons.FirstOrDefaultAsync(c=>c.Code==code);
 public async Task SaveCouponAsync(Coupon coupon)
 {
  if(coupon.Id==0) db.Coupons.Add(coupon);
  else { var current=await db.Coupons.FindAsync(coupon.Id); if(current is null) return; db.Entry(current).CurrentValues.SetValues(coupon); }
  await db.SaveChangesAsync();
 }
 public Task<List<int>> WishlistAsync(string email) => db.WishlistItems.Where(w=>w.UserEmail==email).Select(w=>w.ProductId).ToListAsync();
 public async Task ToggleWishAsync(string email,int productId)
 {
  var item=await db.WishlistItems.FirstOrDefaultAsync(w=>w.UserEmail==email && w.ProductId==productId);
  if(item is null) db.WishlistItems.Add(new WishlistItem{UserEmail=email,ProductId=productId}); else db.WishlistItems.Remove(item);
  await db.SaveChangesAsync();
 }
 public Task<List<Order>> AllOrdersAsync()=>db.Orders.Include(o=>o.Lines).OrderByDescending(o=>o.CreatedAt).ToListAsync();
 public async Task SetStatusAsync(int id,string status)
 {
  var order=await db.Orders.FindAsync(id); if(order is null)return;
  string[] steps=["Confirmed","Processing","Shipped","Delivered"];
  if(order.Status=="Cancelled"||Array.IndexOf(steps,order.Status)<0||Array.IndexOf(steps,status)!=Array.IndexOf(steps,order.Status)+1) throw new InvalidOperationException("Invalid status transition");
  order.Status=status;
  if(status=="Delivered"&&order.PaymentMethod=="COD")order.PaymentStatus="CollectedDemo";await db.SaveChangesAsync();
 }
 public Task<List<ShippingZone>> ZonesAsync()=>db.ShippingZones.OrderBy(z=>z.Id).ToListAsync();
 public Task<List<ShippingRegion>> RegionsAsync()=>db.ShippingRegions.OrderBy(r=>r.Name).ToListAsync();
 public async Task<ShippingSettings> SettingsAsync()=>await db.ShippingSettings.FirstOrDefaultAsync()??new ShippingSettings{CodEnabled=false};
 public async Task SaveZoneAsync(ShippingZone zone)
 {
  if(zone.Id==0)db.ShippingZones.Add(zone);
  else{var current=await db.ShippingZones.FindAsync(zone.Id);if(current is null)return;db.Entry(current).CurrentValues.SetValues(zone);}
  await db.SaveChangesAsync();
 }
 public async Task SaveSettingsAsync(ShippingSettings settings)
 {
  var current=await db.ShippingSettings.FirstOrDefaultAsync();
  if(current is null){settings.Id=1;db.ShippingSettings.Add(settings);}
  else{current.CodEnabled=settings.CodEnabled;current.CodFee=settings.CodFee;current.CodMaxOrder=settings.CodMaxOrder;}
  await db.SaveChangesAsync();
 }
 public async Task SetRegionZoneAsync(int regionId,int zoneId)
 {
  var region=await db.ShippingRegions.FindAsync(regionId);if(region is null||!await db.ShippingZones.AnyAsync(z=>z.Id==zoneId))return;
  region.ZoneId=zoneId;await db.SaveChangesAsync();
 }
 public Task<Order?> LastOrderAsync(string email)=>db.Orders.Where(o=>o.UserEmail==email&&o.AddressLine!="").OrderByDescending(o=>o.CreatedAt).FirstOrDefaultAsync();
}
