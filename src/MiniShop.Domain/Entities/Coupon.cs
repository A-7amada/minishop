using System.ComponentModel.DataAnnotations;
namespace MiniShop.Domain.Entities;
public class Coupon
{
 public int Id { get; set; }
 [Required, RegularExpression("^[A-Z0-9_-]{3,24}$")] public string Code { get; set; } = "";
 [Range(1,90)] public int Percent { get; set; }
 [Range(0,1000000)] public decimal MinimumSpend { get; set; }
 public bool IsActive { get; set; } = true;
 public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddMonths(1);
}
public class WishlistItem
{
 public int Id {get;set;}
 public string UserEmail {get;set;} = "";
 public int ProductId {get;set;}
}
