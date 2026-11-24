using System.ComponentModel.DataAnnotations;

namespace MiniShop.Domain.Entities;

public class Product
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; } = "";

    [StringLength(100)]
    public string NameAr { get; set; } = "";

    [StringLength(500)]
    public string Description { get; set; } = "";

    [StringLength(500)]
    public string DescriptionAr { get; set; } = "";

    [Range(0.01, 1_000_000)]
    public decimal Price { get; set; }

    [Range(0, 100_000)]
    public int Stock { get; set; }

    // Simple emoji used as the product image so the sample needs no image files.
    public string Emoji { get; set; } = "📦";

    public string Category => Name.Contains("Headphone",StringComparison.OrdinalIgnoreCase)?"Audio":Name.Contains("Monitor",StringComparison.OrdinalIgnoreCase)?"Displays":"Desk";
    public string Artwork => Name.Contains("Mouse",StringComparison.OrdinalIgnoreCase)?"mouse":Name.Contains("Keyboard",StringComparison.OrdinalIgnoreCase)?"keyboard":Name.Contains("Headphone",StringComparison.OrdinalIgnoreCase)?"headphones":Name.Contains("Monitor",StringComparison.OrdinalIgnoreCase)?"monitor":Name.Contains("Hub",StringComparison.OrdinalIgnoreCase)?"hub":"stand";
    public string LocalizedName(bool arabic) => arabic && !string.IsNullOrWhiteSpace(NameAr) ? NameAr : Name;
    public string LocalizedDescription(bool arabic) => arabic && !string.IsNullOrWhiteSpace(DescriptionAr) ? DescriptionAr : Description;
}
