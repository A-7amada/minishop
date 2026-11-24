using MiniShop.Domain.Entities;

namespace MiniShop.Domain.Interfaces;

public interface IOrderRepository
{
    Task AddAsync(Order order);
    Task<List<Order>> ListByUserAsync(string userEmail);
    Task<bool> CancelAsync(string userEmail, int orderId);
}
