using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanHoa.Data;
using WebBanHoa.Models;

namespace WebBanHoa.Controllers;

public class AccountController : Controller
{
    private readonly AppDbContext _db;
    public AccountController(AppDbContext db) => _db = db;

    [HttpGet("tai-khoan/dang-nhap")]
    public IActionResult Login(string? returnUrl) => View(new LoginViewModel { ReturnUrl = returnUrl });

    [HttpPost("tai-khoan/dang-nhap"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);
        var email = vm.Email.Trim().ToLower();
        var c = await _db.Customers.FirstOrDefaultAsync(x => x.Email == email);
        if (c == null || !Passwords.Verify(c.PasswordHash, vm.Password))
        {
            ModelState.AddModelError("", "Email hoặc mật khẩu không đúng");
            return View(vm);
        }
        if (c.IsLocked)
        {
            ModelState.AddModelError("", "Tài khoản của bạn đã bị khóa. Vui lòng liên hệ cửa hàng.");
            return View(vm);
        }
        await SignInAsync(c, vm.RememberMe);
        TempData["Toast"] = $"Xin chào {c.FullName}!";
        return Url.IsLocalUrl(vm.ReturnUrl) ? Redirect(vm.ReturnUrl!) : Redirect("/");
    }

    [HttpGet("tai-khoan/dang-ky")]
    public IActionResult Register() => View(new RegisterViewModel());

    [HttpPost("tai-khoan/dang-ky"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel vm)
    {
        var email = vm.Email?.Trim().ToLower() ?? "";
        if (await _db.Customers.AnyAsync(x => x.Email == email))
            ModelState.AddModelError(nameof(vm.Email), "Email này đã được đăng ký");
        if (!ModelState.IsValid) return View(vm);
        var c = new Customer { FullName = vm.FullName.Trim(), Email = email, Phone = vm.Phone, PasswordHash = Passwords.Hash(vm.Password) };
        _db.Customers.Add(c);
        await _db.SaveChangesAsync();
        await SignInAsync(c, true);
        TempData["Toast"] = "Đăng ký thành công! Chào mừng bạn đến với Floria 🌸";
        return Redirect("/");
    }

    [HttpPost("tai-khoan/dang-xuat"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Redirect("/");
    }

    [Authorize, HttpGet("tai-khoan")]
    public async Task<IActionResult> Profile()
    {
        var c = await _db.Customers.FindAsync(User.CustomerId());
        if (c == null) return await ForceLogout();
        return View(new ProfileViewModel { FullName = c.FullName, Email = c.Email, Phone = c.Phone, Address = c.Address });
    }

    [Authorize, HttpPost("tai-khoan"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(ProfileViewModel vm)
    {
        var c = await _db.Customers.FindAsync(User.CustomerId());
        if (c == null) return await ForceLogout();
        vm.Email = c.Email;
        if (!string.IsNullOrEmpty(vm.NewPassword) && !Passwords.Verify(c.PasswordHash, vm.CurrentPassword ?? ""))
            ModelState.AddModelError(nameof(vm.CurrentPassword), "Mật khẩu hiện tại không đúng");
        if (!ModelState.IsValid) return View(vm);
        c.FullName = vm.FullName.Trim(); c.Phone = vm.Phone; c.Address = vm.Address;
        if (!string.IsNullOrEmpty(vm.NewPassword)) c.PasswordHash = Passwords.Hash(vm.NewPassword);
        await _db.SaveChangesAsync();
        await SignInAsync(c, true);
        TempData["Toast"] = "Đã cập nhật thông tin tài khoản";
        return RedirectToAction(nameof(Profile));
    }

    [Authorize, Route("tai-khoan/don-hang")]
    public async Task<IActionResult> Orders()
    {
        var id = User.CustomerId();
        var orders = await _db.Orders.Include(o => o.Details).Where(o => o.CustomerId == id)
            .OrderByDescending(o => o.CreatedAt).ToListAsync();
        return View(orders);
    }

    [Authorize, Route("tai-khoan/don-hang/{code}")]
    public async Task<IActionResult> OrderDetail(string code)
    {
        var id = User.CustomerId();
        var o = await _db.Orders.Include(x => x.Details).Include(x => x.Transactions)
            .FirstOrDefaultAsync(x => x.Code == code && x.CustomerId == id);
        return o == null ? NotFound() : View("~/Views/Shared/_OrderView.cshtml", o);
    }

    [Authorize, HttpPost("tai-khoan/don-hang/{code}/huy"), ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelOrder(string code)
    {
        var id = User.CustomerId();
        var o = await _db.Orders.Include(x => x.Details).ThenInclude(d => d.Product)
            .FirstOrDefaultAsync(x => x.Code == code && x.CustomerId == id);
        if (o == null) return NotFound();
        if (o.Status != OrderStatus.ChoXacNhan)
            TempData["Toast"] = "Chỉ có thể hủy đơn hàng đang chờ xác nhận";
        else
        {
            OrderOps.Cancel(o, "Khách hàng hủy đơn");
            await _db.SaveChangesAsync();
            TempData["Toast"] = "Đã hủy đơn hàng " + o.Code;
        }
        return RedirectToAction(nameof(OrderDetail), new { code });
    }

    /// <summary>Tra cứu đơn hàng không cần đăng nhập (mã đơn + số điện thoại).</summary>
    [Route("tra-cuu-don-hang")]
    public async Task<IActionResult> Track(string? code, string? phone)
    {
        ViewBag.Code = code; ViewBag.Phone = phone;
        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(phone)) return View((Order?)null);
        var c = code.Trim().ToUpper(); var p = phone.Trim();
        var o = await _db.Orders.Include(x => x.Details).Include(x => x.Transactions)
            .FirstOrDefaultAsync(x => x.Code == c && (x.SenderPhone == p || x.ReceiverPhone == p));
        if (o == null) ViewBag.NotFound = true;
        return View(o);
    }

    private async Task SignInAsync(Customer c, bool persistent)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, c.Id.ToString()),
            new(ClaimTypes.Name, c.FullName),
            new(ClaimTypes.Email, c.Email),
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties { IsPersistent = persistent });
    }

    private async Task<IActionResult> ForceLogout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }
}

public static class OrderOps
{
    /// <summary>Hủy đơn và hoàn lại tồn kho.</summary>
    public static void Cancel(Order o, string reason)
    {
        if (o.Status == OrderStatus.DaHuy) return;
        foreach (var d in o.Details.Where(d => d.Product != null))
        {
            d.Product!.Stock += d.Quantity;
            d.Product.Sold = Math.Max(0, d.Product.Sold - d.Quantity);
        }
        o.Status = OrderStatus.DaHuy;
        o.CancelReason = reason;
        o.UpdatedAt = DateTime.UtcNow;
    }
}
