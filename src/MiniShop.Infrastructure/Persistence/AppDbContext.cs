using Microsoft.EntityFrameworkCore;
using MiniShop.Domain.Entities;

namespace MiniShop.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder model)
    {
        model.Entity<Coupon>().HasIndex(c => c.Code).IsUnique();
        model.Entity<WishlistItem>().HasIndex(w => new {w.UserEmail, w.ProductId}).IsUnique();
    }
    public DbSet<ShippingZone> ShippingZones => Set<ShippingZone>();
    public DbSet<ShippingRegion> ShippingRegions => Set<ShippingRegion>();
    public DbSet<ShippingSettings> ShippingSettings => Set<ShippingSettings>();
    public DbSet<Coupon> Coupons => Set<Coupon>();
    public DbSet<WishlistItem> WishlistItems => Set<WishlistItem>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderLine> OrderLines => Set<OrderLine>();
}
