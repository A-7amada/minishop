using System.Text.RegularExpressions;
using MiniShop.Application.DTOs;
using MiniShop.Domain.Entities;
using MiniShop.Domain.Interfaces;
namespace MiniShop.Application.Services;
public class OrderService(IOrderRepository orders,IProductRepository products,ShopService shop,ShippingService shipping)
{
 public async Task<Order> CreateFromCartAsync(string email,List<CartLine> cart,string? couponCode,CheckoutDraft draft)
 {
  var name=draft.FullName?.Trim()??"";var phone=draft.Phone?.Trim()??"";var city=draft.City?.Trim()??"";var line=draft.Line?.Trim()??"";
  if(name.Length<2||name.Length>80||city.Length<2||city.Length>60||line.Length<5||line.Length>200||!Regex.IsMatch(phone,@"^\+?[0-9 \-]{8,16}$"))throw new InvalidOperationException("AddressRequired");
  var payment=draft.Payment==ShippingService.Cod?ShippingService.Cod:ShippingService.Mock;
  var order=new Order{UserEmail=email};
  foreach(var l in cart)
  {
   var product=await products.FindAsync(l.ProductId);
   if(product is null || l.Quantity<1 || l.Quantity>product.Stock)throw new InvalidOperationException("StockChanged");
   order.Lines.Add(new OrderLine{ProductId=product.Id,ProductName=product.Name,ProductNameAr=product.NameAr,UnitPrice=product.Price,Quantity=l.Quantity});
  }
  order.Subtotal=order.Lines.Sum(x=>x.UnitPrice*x.Quantity);
  var result=await shop.DiscountAsync(couponCode,order.Subtotal);
  if(result.error is not null)throw new InvalidOperationException(result.error);
  order.Discount=result.discount;order.CouponCode=string.IsNullOrWhiteSpace(couponCode)?null:couponCode.Trim().ToUpperInvariant();
  var quote=await shipping.QuoteAsync(draft.RegionId,order.Subtotal-order.Discount,payment);
  if(!quote.Ok)throw new InvalidOperationException(quote.Error??"SelectRegion");
  order.ShippingFee=quote.Fee;order.CodFee=quote.CodFee;order.ShippingZone=quote.ZoneName;order.Governorate=quote.RegionName;order.GovernorateAr=quote.RegionNameAr;
  order.FullName=name;order.Phone=phone;order.City=city;order.AddressLine=line;order.PaymentMethod=payment;
  order.PaymentStatus=payment==ShippingService.Cod?"DueOnDelivery":"PaidDemo";
  order.Total=order.Subtotal-order.Discount+order.ShippingFee+order.CodFee;
  await orders.AddAsync(order);return order;
 }
 public Task<bool> CancelAsync(string email,int id)=>orders.CancelAsync(email,id);
 public Task<List<Order>> ListForUserAsync(string email)=>orders.ListByUserAsync(email);
}
