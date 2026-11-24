using MiniShop.Application.DTOs;

namespace MiniShop.Application.Interfaces;

// Persistence for the cart. The Web layer implements this with the HTTP session.
public interface ICartStore
{
    List<CartLine> Load();
    void Save(List<CartLine> cart);
    void Clear();
}
