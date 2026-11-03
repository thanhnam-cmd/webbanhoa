using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanHoa.Data;
using WebBanHoa.Infrastructure;
using WebBanHoa.Models;

namespace WebBanHoa.Areas.Admin.Controllers;

public class DashboardViewModel
{
    public decimal Revenue { get; set; }
    public decimal RevenueThisMonth { get; set; }
    public int Orders { get; set; }
    public int PendingOrders { get; set; }
    public int Products { get; set; }
    public int Customers { get; set; }
    public int UnreadContacts { get; set; }
    public List<string> MonthLabels { get; set; } = new();
    public List<decimal> MonthRevenue { get; set; } = new();
    public Dictionary<string, int> StatusCounts { get; set; } = new();
    public List<(string Name, string? Image, int Qty, decimal Amount)> TopProducts { get; set; } = new();
    public List<Order> LatestOrders { get; set; } = new();
    public List<Product> LowStock { get; set; } = new();
}

public class DashboardController : AdminBaseController
{
    public DashboardController(AppDbContext db) : base(db) { }

    [Route("admin")]
    [Route("admin/dashboard")]
    public async Task<IActionResult> Index()
    {
        // Doanh thu tính trên đơn hoàn thành hoặc đã thanh toán online (không tính đơn hủy)
        var valid = Db.Orders.Where(o => o.Status != OrderStatus.DaHuy && (o.Status == OrderStatus.HoanThanh || o.PaymentStatus == PaymentStatus.DaThanhToan));
        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1).AddMonths(-5);
        var recent = await valid.Where(o => o.CreatedAt >= monthStart).Select(o => new { o.CreatedAt, o.Total }).ToListAsync();

        var vm = new DashboardViewModel
        {
            Revenue = await valid.SumAsync(o => (decimal?)o.Total) ?? 0,
            RevenueThisMonth = recent.Where(o => o.CreatedAt.Year == now.Year && o.CreatedAt.Month == now.Month).Sum(o => o.Total),
            Orders = await Db.Orders.CountAsync(),
            PendingOrders = await Db.Orders.CountAsync(o => o.Status == OrderStatus.ChoXacNhan),
            Products = await Db.Products.CountAsync(),
            Customers = await Db.Customers.CountAsync(),
            UnreadContacts = await Db.ContactMessages.CountAsync(c => !c.IsRead),
            LatestOrders = await Db.Orders.OrderByDescending(o => o.CreatedAt).Take(6).ToListAsync(),
            LowStock = await Db.Products.Where(p => p.Stock < 20).OrderBy(p => p.Stock).Take(5).ToListAsync(),
        };
        for (var i = 0; i < 6; i++)
        {
            var m = monthStart.AddMonths(i);
            vm.MonthLabels.Add($"T{m.Month}/{m:yy}");
            vm.MonthRevenue.Add(recent.Where(o => o.CreatedAt.Year == m.Year && o.CreatedAt.Month == m.Month).Sum(o => o.Total));
        }
        var statuses = await Db.Orders.GroupBy(o => o.Status).Select(g => new { g.Key, C = g.Count() }).ToListAsync();
        foreach (OrderStatus s in Enum.GetValues<OrderStatus>())
            vm.StatusCounts[s.Label()] = statuses.FirstOrDefault(x => x.Key == s)?.C ?? 0;

        var top = await Db.OrderDetails.Where(d => d.Order!.Status != OrderStatus.DaHuy)
            .GroupBy(d => new { d.ProductName, d.Image })
            .Select(g => new { g.Key.ProductName, g.Key.Image, Qty = g.Sum(x => x.Quantity), Amount = g.Sum(x => x.Price * x.Quantity) })
            .OrderByDescending(x => x.Qty).Take(5).ToListAsync();
        vm.TopProducts = top.Select(x => (x.ProductName, x.Image, x.Qty, x.Amount)).ToList();
        return View(vm);
    }
}
