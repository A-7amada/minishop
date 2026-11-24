using System.Text.Json;
using MiniShop.Application.DTOs;
using MiniShop.Application.Interfaces;

namespace MiniShop.Web.Services;

// Stores the cart in the HTTP session as JSON.
public class SessionCartStore : ICartStore
{
    private const string Key = "cart";
    private readonly IHttpContextAccessor _http;
    public SessionCartStore(IHttpContextAccessor http) => _http = http;

    private ISession Session => _http.HttpContext!.Session;

    public List<CartLine> Load()
    {
        var data = Session.GetString(Key);
        return data is null ? new List<CartLine>() : JsonSerializer.Deserialize<List<CartLine>>(data) ?? new List<CartLine>();
    }

    public void Save(List<CartLine> cart) => Session.SetString(Key, JsonSerializer.Serialize(cart));

    public void Clear() => Session.Remove(Key);
}
