using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanHoa.Data;
using WebBanHoa.Infrastructure;
using WebBanHoa.Services;

namespace WebBanHoa.Controllers;

public class CartController : Controller
{
    private readonly CartService _cart;
    private readonly AppDbContext _db;
    public CartController(CartService cart, AppDbContext db) { _cart = cart; _db = db; }

    [Route("gio-hang")]
    public async Task<IActionResult> Index() => View(await _cart.GetViewAsync());

    [HttpPost("gio-hang/them"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(int productId, int quantity = 1, bool buyNow = false)
    {
        var p = await _db.Products.FirstOrDefaultAsync(x => x.Id == productId && x.IsActive);
        if (p == null) return NotFound();
        if (p.Stock <= 0)
            return IsAjax() ? Json(new { ok = false, message = "Sản phẩm tạm hết hàng" }) : RedirectToAction(nameof(Index));
        _cart.Add(productId, quantity);
        if (IsAjax()) return Json(new { ok = true, count = _cart.Count(), message = $"Đã thêm “{p.Name}” vào giỏ hàng" });
        return buyNow ? RedirectToAction("Index", "Checkout") : RedirectToAction(nameof(Index));
    }

    [HttpPost("gio-hang/cap-nhat"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(int productId, int quantity)
    {
        _cart.Update(productId, quantity);
        if (!IsAjax()) return RedirectToAction(nameof(Index));
        var v = await _cart.GetViewAsync();
        var line = v.Lines.FirstOrDefault(l => l.Product.Id == productId);
        return Json(new
        {
            ok = true, count = v.Count,
            line = line?.LineTotal.ToVnd(), subtotal = v.Subtotal.ToVnd(), shipping = v.ShippingFee == 0 ? "Miễn phí" : v.ShippingFee.ToVnd(),
            total = v.Total.ToVnd(), freeShipLeft = Math.Max(0, CartService.FreeShipThreshold - v.Subtotal).ToVnd(),
            freeShipPercent = (int)Math.Min(100, v.Subtotal / CartService.FreeShipThreshold * 100)
        });
    }

    [HttpPost("gio-hang/xoa"), ValidateAntiForgeryToken]
    public IActionResult Remove(int productId)
    {
        _cart.Remove(productId);
        return RedirectToAction(nameof(Index));
    }

    [Route("gio-hang/so-luong")]
    public IActionResult Count() => Json(new { count = _cart.Count() });

    private bool IsAjax() => Request.Headers.XRequestedWith == "XMLHttpRequest";
}
