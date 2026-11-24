namespace MiniShop.Application.DTOs;

// One line in the shopping cart.
public class CartLine
{
    public int ProductId { get; set; }
    public string Name { get; set; } = "";
    public string NameAr { get; set; } = "";
    public decimal Price { get; set; }
    public int Quantity { get; set; }

    public string LocalizedName(bool arabic) => arabic && !string.IsNullOrWhiteSpace(NameAr) ? NameAr : Name;
    public decimal Subtotal => Price * Quantity;
}
