using System.ComponentModel.DataAnnotations;
namespace MiniShop.Domain.Entities;
public class ShippingZone
{
 public int Id { get; set; }
 [Required, StringLength(60,MinimumLength=2)] public string Name { get; set; } = "";
 [Required, StringLength(60,MinimumLength=2)] public string NameAr { get; set; } = "";
 [Range(0,100000)] public decimal Fee { get; set; }
 [Range(0,1000000)] public decimal FreeThreshold { get; set; }
 [Range(0,60)] public int MinDays { get; set; } = 1;
 [Range(0,60)] public int MaxDays { get; set; } = 3;
 public bool IsActive { get; set; } = true;
}
public class ShippingRegion
{
 public int Id { get; set; }
 public string Name { get; set; } = "";
 public string NameAr { get; set; } = "";
 public int ZoneId { get; set; }
}
public class ShippingSettings
{
 public int Id { get; set; } = 1;
 public bool CodEnabled { get; set; } = true;
 [Range(0,100000)] public decimal CodFee { get; set; }
 [Range(0,10000000)] public decimal CodMaxOrder { get; set; }
}
