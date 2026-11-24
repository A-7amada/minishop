using Microsoft.EntityFrameworkCore;
using MiniShop.Domain.Entities;
using MiniShop.Domain.Interfaces;
using MiniShop.Infrastructure.Persistence;

namespace MiniShop.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _db;
    public OrderRepository(AppDbContext db) => _db = db;

    public async Task AddAsync(Order order)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync();
        foreach (var line in order.Lines)
        {
            var updated = await _db.Products.Where(p => p.Id == line.ProductId && p.Stock >= line.Quantity)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.Stock, p => p.Stock - line.Quantity));
            if (updated != 1) throw new InvalidOperationException("StockChanged");
        }
        _db.Orders.Add(order);
        await _db.SaveChangesAsync();
        await transaction.CommitAsync();
    }

    // Customers may cancel only their own order while it is still "Confirmed". Stock is returned in the same transaction.
    public async Task<bool> CancelAsync(string userEmail, int orderId)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync();
        var order = await _db.Orders.Include(o => o.Lines).FirstOrDefaultAsync(o => o.Id == orderId && o.UserEmail == userEmail);
        if (order is null) return false;
        var changed = await _db.Orders.Where(o => o.Id == orderId && o.Status == "Confirmed")
            .ExecuteUpdateAsync(s => s.SetProperty(o => o.Status, "Cancelled")
                .SetProperty(o => o.PaymentStatus, order.PaymentMethod == "COD" ? "CancelledPay" : "RefundedDemo"));
        if (changed != 1) return false;
        foreach (var line in order.Lines)
        {
            var qty = line.Quantity;
            await _db.Products.Where(p => p.Id == line.ProductId).ExecuteUpdateAsync(s => s.SetProperty(p => p.Stock, p => p.Stock + qty));
        }
        await transaction.CommitAsync();
        return true;
    }

    public Task<List<Order>> ListByUserAsync(string userEmail) =>
        _db.Orders.Include(o => o.Lines)
            .Where(o => o.UserEmail == userEmail)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
}
