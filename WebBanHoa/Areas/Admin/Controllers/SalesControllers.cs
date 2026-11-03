using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanHoa.Controllers;
using WebBanHoa.Data;
using WebBanHoa.Infrastructure;
using WebBanHoa.Models;

namespace WebBanHoa.Areas.Admin.Controllers;

// ======================= Đơn hàng =======================
public class OrdersController : AdminBaseController
{
    public OrdersController(AppDbContext db) : base(db) { }

    public async Task<IActionResult> Index(string? q, OrderStatus? status, int page = 1)
    {
        var query = Db.Orders.Include(o => o.Details).AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(o => o.Code.Contains(q) || o.SenderName.Contains(q) || o.SenderPhone.Contains(q) || o.ReceiverName.Contains(q) || o.ReceiverPhone.Contains(q));
        if (status.HasValue) query = query.Where(o => o.Status == status);
        ViewBag.Status = status;
        ViewBag.Counts = await Db.Orders.GroupBy(o => o.Status).Select(g => new { g.Key, C = g.Count() }).ToDictionaryAsync(x => x.Key, x => x.C);
        return View(await PageAsync(query.OrderByDescending(o => o.CreatedAt), page, q));
    }

    public async Task<IActionResult> Detail(int id)
    {
        var o = await Db.Orders.Include(x => x.Details).Include(x => x.Transactions).Include(x => x.Customer).FirstOrDefaultAsync(x => x.Id == id);
        return o == null ? NotFound() : View(o);
    }

    /// <summary>Duyệt đơn theo quy trình: Chờ xác nhận → Đã xác nhận → Đang giao → Hoàn thành.</summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, OrderStatus status, string? reason)
    {
        var o = await Db.Orders.Include(x => x.Details).ThenInclude(d => d.Product).FirstOrDefaultAsync(x => x.Id == id);
        if (o == null) return NotFound();
        if (o.Status is OrderStatus.DaHuy or OrderStatus.HoanThanh)
        {
            Toast("Đơn hàng đã kết thúc, không thể cập nhật", "danger");
            return RedirectToAction(nameof(Detail), new { id });
        }
        if (status == OrderStatus.DaHuy) OrderOps.Cancel(o, string.IsNullOrWhiteSpace(reason) ? "Cửa hàng hủy đơn" : reason);
        else
        {
            o.Status = status;
            if (status == OrderStatus.HoanThanh && o.PaymentMethod == PaymentMethod.COD) o.PaymentStatus = PaymentStatus.DaThanhToan;
            o.UpdatedAt = DateTime.UtcNow;
        }
        await Db.SaveChangesAsync();
        Toast($"Đơn {o.Code}: {o.Status.Label()}");
        return RedirectToAction(nameof(Detail), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkPaid(int id)
    {
        var o = await Db.Orders.FindAsync(id);
        if (o == null) return NotFound();
        o.PaymentStatus = PaymentStatus.DaThanhToan;
        o.UpdatedAt = DateTime.UtcNow;
        await Db.SaveChangesAsync();
        Toast("Đã xác nhận thanh toán");
        return RedirectToAction(nameof(Detail), new { id });
    }

    public async Task<IActionResult> Print(int id)
    {
        var o = await Db.Orders.Include(x => x.Details).FirstOrDefaultAsync(x => x.Id == id);
        return o == null ? NotFound() : View(o);
    }
}

// ======================= Khách hàng =======================
public class CustomersController : AdminBaseController
{
    public CustomersController(AppDbContext db) : base(db) { }

    public async Task<IActionResult> Index(string? q, int page = 1)
    {
        var query = Db.Customers.Include(c => c.Orders).AsQueryable();
        if (!string.IsNullOrWhiteSpace(q)) query = query.Where(c => c.FullName.Contains(q) || c.Email.Contains(q) || (c.Phone != null && c.Phone.Contains(q)));
        return View(await PageAsync(query.OrderByDescending(c => c.CreatedAt), page, q));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleLock(int id)
    {
        var c = await Db.Customers.FindAsync(id);
        if (c == null) return NotFound();
        c.IsLocked = !c.IsLocked;
        await Db.SaveChangesAsync();
        Toast(c.IsLocked ? $"Đã khóa tài khoản {c.Email}" : $"Đã mở khóa tài khoản {c.Email}");
        return RedirectToAction(nameof(Index));
    }
}

// ======================= Liên hệ =======================
public class ContactsController : AdminBaseController
{
    public ContactsController(AppDbContext db) : base(db) { }

    public async Task<IActionResult> Index() => View(await Db.ContactMessages.OrderBy(c => c.IsRead).ThenByDescending(c => c.CreatedAt).ToListAsync());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkRead(int id)
    {
        var c = await Db.ContactMessages.FindAsync(id);
        if (c == null) return NotFound();
        c.IsRead = true;
        await Db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var c = await Db.ContactMessages.FindAsync(id);
        if (c == null) return NotFound();
        Db.ContactMessages.Remove(c);
        await Db.SaveChangesAsync();
        Toast("Đã xóa liên hệ");
        return RedirectToAction(nameof(Index));
    }
}
