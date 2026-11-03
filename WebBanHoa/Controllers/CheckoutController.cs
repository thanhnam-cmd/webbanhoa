using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanHoa.Data;
using WebBanHoa.Infrastructure;
using WebBanHoa.Models;
using WebBanHoa.Services;

namespace WebBanHoa.Controllers;

public class CheckoutController : Controller
{
    private readonly AppDbContext _db;
    private readonly CartService _cart;
    private readonly VnPayService _vnPay;

    public CheckoutController(AppDbContext db, CartService cart, VnPayService vnPay)
    {
        _db = db; _cart = cart; _vnPay = vnPay;
    }

    [HttpGet("thanh-toan")]
    public async Task<IActionResult> Index()
    {
        var cart = await _cart.GetViewAsync();
        if (cart.Lines.Count == 0) return RedirectToAction("Index", "Cart");
        var slot = NextSlot();
        var vm = new CheckoutViewModel
        {
            Cart = cart,
            DeliveryDate = slot == null ? Helpers.VnNow.Date.AddDays(1) : Helpers.VnNow.Date,
            DeliverySlot = slot ?? Helpers.DeliverySlots[0],
        };
        var cid = User.CustomerId();
        if (cid != null)
        {
            var c = await _db.Customers.FindAsync(cid);
            if (c != null) { vm.SenderName = c.FullName; vm.SenderPhone = c.Phone ?? ""; vm.SenderEmail = c.Email; vm.Address = c.Address ?? ""; }
        }
        return View(vm);
    }

    [HttpPost("thanh-toan"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(CheckoutViewModel vm)
    {
        var cart = await _cart.GetViewAsync();
        if (cart.Lines.Count == 0) return RedirectToAction("Index", "Cart");
        vm.Cart = cart;

        if (vm.DeliveryDate.Date < Helpers.VnNow.Date)
            ModelState.AddModelError(nameof(vm.DeliveryDate), "Ngày giao không được ở quá khứ");
        else if (vm.DeliveryDate.Date == Helpers.VnNow.Date && !string.IsNullOrEmpty(vm.DeliverySlot)
                 && int.TryParse(vm.DeliverySlot[..2], out var startHour) && startHour < Helpers.VnNow.Hour + 1)
            ModelState.AddModelError(nameof(vm.DeliverySlot), "Khung giờ này đã qua, vui lòng chọn khung giờ hoặc ngày khác");
        foreach (var l in cart.Lines.Where(l => l.Quantity > l.Product.Stock))
            ModelState.AddModelError("", $"“{l.Product.Name}” chỉ còn {l.Product.Stock} sản phẩm");
        if (!ModelState.IsValid) return View(vm);

        var order = new Order
        {
            Code = await NewCodeAsync(),
            CustomerId = User.CustomerId(),
            SenderName = vm.SenderName.Trim(), SenderPhone = vm.SenderPhone.Trim(), SenderEmail = vm.SenderEmail?.Trim(),
            ReceiverName = vm.ReceiverName.Trim(), ReceiverPhone = vm.ReceiverPhone.Trim(), Address = vm.Address.Trim(),
            DeliveryDate = vm.DeliveryDate.Date, DeliverySlot = vm.DeliverySlot,
            CardMessage = vm.CardMessage, Note = vm.Note,
            PaymentMethod = vm.PaymentMethod,
            Subtotal = cart.Subtotal, ShippingFee = cart.ShippingFee, Total = cart.Total,
        };
        foreach (var l in cart.Lines)
        {
            order.Details.Add(new OrderDetail { ProductId = l.Product.Id, ProductName = l.Product.Name, Image = l.Product.Image, Price = l.Product.FinalPrice, Quantity = l.Quantity });
            l.Product.Stock -= l.Quantity;
            l.Product.Sold += l.Quantity;
        }
        _db.Orders.Add(order);
        await _db.SaveChangesAsync();
        _cart.Clear();
        HttpContext.Session.SetString(LastOrderKey, order.Code);

        if (order.PaymentMethod == PaymentMethod.VNPay)
            return Redirect(BuildPaymentUrl(order));

        return RedirectToAction(nameof(Success), new { code = order.Code });
    }

    [Route("dat-hang-thanh-cong/{code}")]
    public async Task<IActionResult> Success(string code)
    {
        var order = await _db.Orders.Include(o => o.Details).FirstOrDefaultAsync(o => o.Code == code);
        if (order == null) return NotFound();
        // Chỉ người vừa đặt đơn (theo session) hoặc chủ tài khoản mới xem được trang này
        var mine = HttpContext.Session.GetString(LastOrderKey) == code || (order.CustomerId != null && order.CustomerId == User.CustomerId());
        return mine ? View(order) : RedirectToAction("Track", "Account", new { code });
    }

    public const string LastOrderKey = "LAST_ORDER";

    /// <summary>Thanh toán lại cho đơn VNPay chưa thanh toán thành công.</summary>
    [Route("thanh-toan-lai/{code}")]
    public async Task<IActionResult> RetryPayment(string code)
    {
        var order = await _db.Orders.FirstOrDefaultAsync(o => o.Code == code);
        if (order == null) return NotFound();
        if (order.PaymentStatus == PaymentStatus.DaThanhToan || order.Status == OrderStatus.DaHuy)
            return RedirectToAction(nameof(Success), new { code });
        order.PaymentMethod = PaymentMethod.VNPay;
        await _db.SaveChangesAsync();
        return Redirect(BuildPaymentUrl(order));
    }

    private string BuildPaymentUrl(Order order)
    {
        var returnUrl = Url.Action("VnPayReturn", "Payment", null, Request.Scheme)!;
        var demoUrl = Url.Action("DemoGateway", "Payment", null, Request.Scheme)!;
        var ip = HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "127.0.0.1";
        return _vnPay.CreatePaymentUrl(order.Code, order.Total, $"Thanh toan don hang {order.Code}", returnUrl, ip, demoUrl);
    }

    private async Task<string> NewCodeAsync()
    {
        while (true)
        {
            var code = "FL" + Helpers.VnNow.ToString("yyMMdd") + Random.Shared.Next(1000, 9999);
            if (!await _db.Orders.AnyAsync(o => o.Code == code)) return code;
        }
    }

    /// <summary>Khung giờ gần nhất còn kịp giao trong hôm nay (cần ~2 giờ chuẩn bị), null nếu đã hết.</summary>
    private static string? NextSlot()
    {
        var h = Helpers.VnNow.Hour + 2;
        return Helpers.DeliverySlots.FirstOrDefault(s => int.Parse(s[..2]) >= h);
    }
}
