namespace MiniShop.Domain.Entities;

public class Order
{
    public int Id { get; set; }
    public string UserEmail { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public decimal Total { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Discount { get; set; }
    public string? CouponCode { get; set; }
    public string Status { get; set; } = "Confirmed";
    public decimal ShippingFee { get; set; }
    public decimal CodFee { get; set; }
    public string ShippingZone { get; set; } = "";
    public string PaymentMethod { get; set; } = "Mock";
    public string PaymentStatus { get; set; } = "PaidDemo";
    public string FullName { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Governorate { get; set; } = "";
    public string GovernorateAr { get; set; } = "";
    public string City { get; set; } = "";
    public string AddressLine { get; set; } = "";
    public List<OrderLine> Lines { get; set; } = new();
}

public class OrderLine
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = "";
    public string ProductNameAr { get; set; } = "";
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
}
