using MiniShop.Application.DTOs;
using MiniShop.Domain.Entities;
using MiniShop.Domain.Interfaces;
namespace MiniShop.Application.Services;
public class ShippingService(IShopRepository shop)
{
 public const string Mock="Mock",Cod="COD";
 public Task<List<ShippingRegion>> RegionsAsync()=>shop.RegionsAsync();
 public Task<List<ShippingZone>> ZonesAsync()=>shop.ZonesAsync();
 public Task<ShippingSettings> SettingsAsync()=>shop.SettingsAsync();
 public async Task SaveZoneAsync(ShippingZone z){z.Name=z.Name.Trim();z.NameAr=z.NameAr.Trim();if(z.MaxDays<z.MinDays)throw new InvalidOperationException("InvalidFields");await shop.SaveZoneAsync(z);}
 public Task SaveSettingsAsync(ShippingSettings s)=>shop.SaveSettingsAsync(s);
 public Task SetRegionZoneAsync(int regionId,int zoneId)=>shop.SetRegionZoneAsync(regionId,zoneId);
 public Task<Order?> LastOrderAsync(string email)=>shop.LastOrderAsync(email);
 // Amazon.eg / Jumia pattern: fee depends on destination zone, waived above a per-zone basket threshold (measured after coupon discount).
 // Cash on delivery adds an optional configurable fee and an optional order-value cap.
 public async Task<ShippingQuote> QuoteAsync(int regionId,decimal basket,string? payment)
 {
  var empty=new ShippingQuote(false,null,0,0,false,"","",0,0,0,0,"","");
  var region=(await shop.RegionsAsync()).FirstOrDefault(r=>r.Id==regionId);
  if(region is null)return empty with{Error="SelectRegion"};
  var zone=(await shop.ZonesAsync()).FirstOrDefault(z=>z.Id==region.ZoneId);
  if(zone is null||!zone.IsActive)return empty with{Error="RegionUnavailable",RegionName=region.Name,RegionNameAr=region.NameAr};
  var free=zone.FreeThreshold>0&&basket>=zone.FreeThreshold;
  var fee=free?0m:zone.Fee;
  var toFree=zone.FreeThreshold>0&&!free?zone.FreeThreshold-basket:0m;
  decimal cod=0;string? err=null;
  if(payment==Cod)
  {
   var s=await shop.SettingsAsync();
   if(!s.CodEnabled)err="CodUnavailable";
   else if(s.CodMaxOrder>0&&basket+fee>s.CodMaxOrder)err="CodLimit";
   else cod=s.CodFee;
  }
  return new ShippingQuote(err is null,err,fee,cod,free,zone.Name,zone.NameAr,zone.MinDays,zone.MaxDays,zone.FreeThreshold,toFree,region.Name,region.NameAr);
 }
}
