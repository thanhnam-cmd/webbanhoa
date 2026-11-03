using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using WebBanHoa.Data;
using WebBanHoa.Models;

namespace WebBanHoa.Services;

public class CartItem
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}

public class CartLine
{
    public Product Product { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal LineTotal => Product.FinalPrice * Quantity;
}

public class CartView
{
    public List<CartLine> Lines { get; set; } = new();
    public decimal Subtotal => Lines.Sum(l => l.LineTotal);
    public decimal ShippingFee => Lines.Count == 0 || Subtotal >= CartService.FreeShipThreshold ? 0 : CartService.ShippingFee;
    public decimal Total => Subtotal + ShippingFee;
    public int Count => Lines.Sum(l => l.Quantity);
}

/// <summary>Giỏ hàng lưu trong Session dưới dạng JSON (ProductId + số lượng).</summary>
public class CartService
{
    public const decimal ShippingFee = 30000;
    public const decimal FreeShipThreshold = 500000;
    private const string Key = "CART";
    private readonly IHttpContextAccessor _http;
    private readonly AppDbContext _db;

    public CartService(IHttpContextAccessor http, AppDbContext db) { _http = http; _db = db; }

    private ISession Session => _http.HttpContext!.Session;

    public List<CartItem> Items()
    {
        var json = Session.GetString(Key);
        return string.IsNullOrEmpty(json) ? new() : JsonSerializer.Deserialize<List<CartItem>>(json) ?? new();
    }

    private void Save(List<CartItem> items) => Session.SetString(Key, JsonSerializer.Serialize(items));

    public int Count() => Items().Sum(i => i.Quantity);

    public void Add(int productId, int qty)
    {
        var items = Items();
        var it = items.FirstOrDefault(i => i.ProductId == productId);
        if (it == null) items.Add(new CartItem { ProductId = productId, Quantity = Math.Clamp(qty, 1, 99) });
        else it.Quantity = Math.Clamp(it.Quantity + qty, 1, 99);
        Save(items);
    }

    public void Update(int productId, int qty)
    {
        var items = Items();
        var it = items.FirstOrDefault(i => i.ProductId == productId);
        if (it == null) return;
        if (qty <= 0) items.Remove(it); else it.Quantity = Math.Min(qty, 99);
        Save(items);
    }

    public void Remove(int productId) => Update(productId, 0);
    public void Clear() => Session.Remove(Key);

    public async Task<CartView> GetViewAsync()
    {
        var items = Items();
        var ids = items.Select(i => i.ProductId).ToList();
        var products = await _db.Products.Where(p => ids.Contains(p.Id) && p.IsActive).ToListAsync();
        var view = new CartView();
        foreach (var i in items)
        {
            var p = products.FirstOrDefault(x => x.Id == i.ProductId);
            if (p != null) view.Lines.Add(new CartLine { Product = p, Quantity = i.Quantity });
        }
        return view;
    }
}
