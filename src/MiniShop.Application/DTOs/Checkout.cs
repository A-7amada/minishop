namespace MiniShop.Application.DTOs;
public record CheckoutDraft(string? FullName,string? Phone,int RegionId,string? City,string? Line,string? Payment);
public record ShippingQuote(bool Ok,string? Error,decimal Fee,decimal CodFee,bool Free,string ZoneName,string ZoneNameAr,int MinDays,int MaxDays,decimal FreeThreshold,decimal AmountToFree,string RegionName,string RegionNameAr);
