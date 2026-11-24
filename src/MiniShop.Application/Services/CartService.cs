using MiniShop.Application.DTOs;
using MiniShop.Application.Interfaces;
using MiniShop.Domain.Entities;

namespace MiniShop.Application.Services;

// Cart logic; storage is abstracted behind ICartStore (session in the Web layer).
public class CartService
{
    private readonly ICartStore _store;
    public CartService(ICartStore store) => _store = store;

    public List<CartLine> Get() => _store.Load();

    public void Add(Product product)
    {
        var cart = Get();
        var line = cart.FirstOrDefault(l => l.ProductId == product.Id);
        if (line is null)
            cart.Add(new CartLine { ProductId = product.Id, Name = product.Name, NameAr = product.NameAr, Price = product.Price, Quantity = 1 });
        else
            line.Quantity++;
        _store.Save(cart);
    }

    public void Update(int id,int quantity,int stock)
    {
        var lines=Get();var line=lines.FirstOrDefault(l=>l.ProductId==id);
        if(line is not null)line.Quantity=Math.Clamp(quantity,1,Math.Max(1,stock));_store.Save(lines);
    }
    public void Remove(int productId) => _store.Save(Get().Where(l => l.ProductId != productId).ToList());
    public void Clear() => _store.Clear();

    public int Count => Get().Sum(l => l.Quantity);
    public decimal Total => Get().Sum(l => l.Subtotal);
}
