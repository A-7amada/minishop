using MiniShop.Domain.Entities;
using MiniShop.Domain.Interfaces;

namespace MiniShop.Application.Services;

public class CatalogService
{
    private readonly IProductRepository _products;
    public CatalogService(IProductRepository products) => _products = products;

    public Task<List<Product>> SearchAsync(string? query) => _products.SearchAsync(query);
    public Task<Product?> GetAsync(int id) => _products.FindAsync(id);
    public Task CreateAsync(Product product) => _products.AddAsync(product);
    public Task UpdateAsync(Product product) => _products.UpdateAsync(product);
    public Task DeleteAsync(int id) => _products.DeleteAsync(id);
}
