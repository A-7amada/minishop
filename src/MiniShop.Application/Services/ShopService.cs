using MiniShop.Domain.Entities;
using MiniShop.Domain.Interfaces;
namespace MiniShop.Application.Services;
public class ShopService(IShopRepository shop, IProductRepository products)
{
 public Task<List<Coupon>> CouponsAsync()=>shop.CouponsAsync();
 public async Task SaveCouponAsync(Coupon coupon)
 {
  coupon.Code=coupon.Code.Trim().ToUpperInvariant();
  var existing=await shop.CouponAsync(coupon.Code);
  if(existing is not null && existing.Id!=coupon.Id)throw new InvalidOperationException("DuplicateCode");
  await shop.SaveCouponAsync(coupon);
 }
 public async Task<(decimal discount,string? error)> DiscountAsync(string? code,decimal subtotal)
 {
  if(string.IsNullOrWhiteSpace(code)) return (0,null);
  var coupon=await shop.CouponAsync(code.Trim().ToUpperInvariant());
  if(coupon is null || !coupon.IsActive || coupon.ExpiresAt<=DateTime.UtcNow)return(0,"InvalidCoupon");
  if(subtotal<coupon.MinimumSpend)return(0,"MinimumNotMet");
  return(decimal.Round(subtotal*coupon.Percent/100m,2,MidpointRounding.AwayFromZero),null);
 }
 public Task<List<int>> WishlistAsync(string email)=>shop.WishlistAsync(email);
 public async Task ToggleWishAsync(string email,int id){if(await products.FindAsync(id) is not null) await shop.ToggleWishAsync(email,id);}
 public Task<List<Order>> AllOrdersAsync()=>shop.AllOrdersAsync();
 public Task SetStatusAsync(int id,string status)=>shop.SetStatusAsync(id,status);
}
