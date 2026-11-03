using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanHoa.Data;
using WebBanHoa.Models;

namespace WebBanHoa.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(AuthenticationSchemes = AuthSchemes.Admin)]
public abstract class AdminBaseController : Controller
{
    protected const int PageSize = 10;
    protected readonly AppDbContext Db;
    protected AdminBaseController(AppDbContext db) => Db = db;

    protected void Toast(string message, string type = "ok")
    {
        TempData["Toast"] = message;
        TempData["ToastType"] = type;
    }

    protected static async Task<PagedList<T>> PageAsync<T>(IQueryable<T> q, int page, string? keyword)
    {
        var total = await q.CountAsync();
        var pages = Math.Max(1, (int)Math.Ceiling(total / (double)PageSize));
        page = Math.Clamp(page, 1, pages);
        return new PagedList<T> { Items = await q.Skip((page - 1) * PageSize).Take(PageSize).ToListAsync(), Page = page, TotalPages = pages, Total = total, Q = keyword };
    }

    /// <summary>Ảnh: ưu tiên file tải lên, nếu không có thì dùng đường dẫn nhập tay.</summary>
    protected async Task<string?> ResolveImageAsync(IFormFile? upload, string? current, Services.MediaService media)
    {
        if (upload == null || upload.Length == 0) return current;
        try { return await media.SaveAsync(upload); }
        catch (InvalidOperationException ex) { ModelState.AddModelError("Image", ex.Message); return current; }
    }

    protected async Task<string> UniqueSlugAsync<T>(IQueryable<T> set, string slug, Func<string, System.Linq.Expressions.Expression<Func<T, bool>>> exists)
    {
        var baseSlug = string.IsNullOrEmpty(slug) ? "muc" : slug;
        var candidate = baseSlug;
        for (var i = 2; await set.AnyAsync(exists(candidate)); i++) candidate = $"{baseSlug}-{i}";
        return candidate;
    }
}
