using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanHoa.Data;
using WebBanHoa.Infrastructure;
using WebBanHoa.Models;

namespace WebBanHoa.Controllers;

public class ShopController : Controller
{
    private const int PageSize = 12;
    private readonly AppDbContext _db;
    public ShopController(AppDbContext db) => _db = db;

    [Route("cua-hang")]
    public async Task<IActionResult> Index(string? q, string? category, string? occasion, decimal? min, decimal? max, string sort = "moi-nhat", int page = 1)
    {
        var vm = new ShopViewModel
        {
            Q = q, Category = category, Occasion = occasion, Min = min, Max = max, Sort = sort,
            Categories = await _db.Categories.Where(c => c.IsActive).OrderBy(c => c.SortOrder).ToListAsync(),
            Occasions = await _db.Occasions.Where(o => o.IsActive).ToListAsync(),
        };

        var query = _db.Products.Include(p => p.Category).Where(p => p.IsActive);

        if (!string.IsNullOrWhiteSpace(q))
        {
            var kw = q.Trim().ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(kw) || (p.ShortDescription != null && p.ShortDescription.ToLower().Contains(kw)));
            vm.Title = $"Kết quả tìm kiếm “{q}”";
        }
        if (!string.IsNullOrEmpty(category))
        {
            query = query.Where(p => p.Category!.Slug == category);
            vm.Title = vm.Categories.FirstOrDefault(c => c.Slug == category)?.Name ?? vm.Title;
        }
        if (!string.IsNullOrEmpty(occasion))
        {
            query = query.Where(p => p.Occasion != null && p.Occasion.Slug == occasion);
            var name = vm.Occasions.FirstOrDefault(o => o.Slug == occasion)?.Name;
            if (name != null) vm.Title = string.IsNullOrEmpty(category) ? $"Hoa {name.ToLower()}" : $"{vm.Title} · {name}";
        }
        // Lọc theo giá bán thực tế (ưu tiên giá khuyến mãi)
        if (min.HasValue) query = query.Where(p => (p.SalePrice != null && p.SalePrice < p.Price ? p.SalePrice : p.Price) >= min);
        if (max.HasValue) query = query.Where(p => (p.SalePrice != null && p.SalePrice < p.Price ? p.SalePrice : p.Price) <= max);

        query = sort switch
        {
            "gia-tang" => query.OrderBy(p => p.SalePrice != null && p.SalePrice < p.Price ? p.SalePrice : p.Price),
            "gia-giam" => query.OrderByDescending(p => p.SalePrice != null && p.SalePrice < p.Price ? p.SalePrice : p.Price),
            "ten-az" => query.OrderBy(p => p.Name),
            "ten-za" => query.OrderByDescending(p => p.Name),
            "ban-chay" => query.OrderByDescending(p => p.Sold),
            _ => query.OrderByDescending(p => p.CreatedAt),
        };

        vm.Total = await query.CountAsync();
        vm.TotalPages = Math.Max(1, (int)Math.Ceiling(vm.Total / (double)PageSize));
        vm.Page = Math.Clamp(page, 1, vm.TotalPages);
        vm.Products = await query.Skip((vm.Page - 1) * PageSize).Take(PageSize).ToListAsync();
        return View(vm);
    }

    [Route("san-pham/{slug}")]
    public async Task<IActionResult> Detail(string slug)
    {
        var p = await _db.Products.Include(x => x.Category).Include(x => x.Occasion)
            .FirstOrDefaultAsync(x => x.Slug == slug && x.IsActive);
        if (p == null) return NotFound();
        var related = await _db.Products.Where(x => x.IsActive && x.Id != p.Id && (x.CategoryId == p.CategoryId || x.OccasionId == p.OccasionId))
            .OrderByDescending(x => x.CategoryId == p.CategoryId).ThenByDescending(x => x.Sold).Take(4).ToListAsync();
        return View(new ProductDetailViewModel { Product = p, Related = related });
    }

    /// <summary>Gợi ý tìm kiếm nhanh trên thanh header (AJAX).</summary>
    [Route("api/tim-kiem")]
    public async Task<IActionResult> Suggest(string q)
    {
        if (string.IsNullOrWhiteSpace(q)) return Json(Array.Empty<object>());
        var kw = q.Trim().ToLower();
        var items = await _db.Products.Where(p => p.IsActive && p.Name.ToLower().Contains(kw))
            .OrderByDescending(p => p.Sold).Take(6).ToListAsync();
        return Json(items.Select(p => new { name = p.Name, url = $"/san-pham/{p.Slug}", image = p.Image.ImageOrDefault(), price = p.FinalPrice.ToVnd() }));
    }
}
