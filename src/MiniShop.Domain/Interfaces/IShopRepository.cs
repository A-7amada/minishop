using MiniShop.Domain.Entities;
namespace MiniShop.Domain.Interfaces;
public interface IShopRepository
{
 Task<List<Coupon>> CouponsAsync();
 Task<Coupon?> CouponAsync(string code);
 Task SaveCouponAsync(Coupon coupon);
 Task<List<int>> WishlistAsync(string email);
 Task ToggleWishAsync(string email, int productId);
 Task<List<Order>> AllOrdersAsync();
 Task SetStatusAsync(int id, string status);
 Task<List<ShippingZone>> ZonesAsync();
 Task<List<ShippingRegion>> RegionsAsync();
 Task<ShippingSettings> SettingsAsync();
 Task SaveZoneAsync(ShippingZone zone);
 Task SaveSettingsAsync(ShippingSettings settings);
 Task SetRegionZoneAsync(int regionId, int zoneId);
 Task<Order?> LastOrderAsync(string email);
}
