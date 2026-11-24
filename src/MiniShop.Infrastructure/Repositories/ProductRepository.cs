using Microsoft.EntityFrameworkCore;
using MiniShop.Domain.Entities;
using MiniShop.Domain.Interfaces;
using MiniShop.Infrastructure.Persistence;

namespace MiniShop.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _db;
    public ProductRepository(AppDbContext db) => _db = db;

    public async Task<List<Product>> SearchAsync(string? query)
    {
        var products = _db.Products.AsQueryable();
        if (!string.IsNullOrWhiteSpace(query))
            products = products.Where(p => p.Name.Contains(query) || p.Description.Contains(query)
                                        || p.NameAr.Contains(query) || p.DescriptionAr.Contains(query));
        return await products.OrderBy(p => p.Name).ToListAsync();
    }

    public async Task<Product?> FindAsync(int id) => await _db.Products.FindAsync(id);

    public async Task AddAsync(Product product)
    {
        _db.Products.Add(product);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Product product)
    {
        _db.Products.Update(product);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var product = await _db.Products.FindAsync(id);
        if (product is not null)
        {
            _db.Products.Remove(product);
            await _db.SaveChangesAsync();
        }
    }
}
