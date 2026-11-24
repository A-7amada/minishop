using MiniShop.Domain.Entities;

namespace MiniShop.Domain.Interfaces;

public interface IProductRepository
{
    Task<List<Product>> SearchAsync(string? query);
    Task<Product?> FindAsync(int id);
    Task AddAsync(Product product);
    Task UpdateAsync(Product product);
    Task DeleteAsync(int id);
}
