using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanHoa.Data;

namespace WebBanHoa.Controllers;

public class BlogController : Controller
{
    private readonly AppDbContext _db;
    public BlogController(AppDbContext db) => _db = db;

    [Route("tin-tuc")]
    public async Task<IActionResult> Index(string? category)
    {
        ViewBag.Categories = await _db.PostCategories.Where(c => c.IsActive).ToListAsync();
        ViewBag.Current = category;
        var q = _db.Posts.Include(p => p.PostCategory).Where(p => p.IsActive);
        if (!string.IsNullOrEmpty(category)) q = q.Where(p => p.PostCategory!.Slug == category);
        return View(await q.OrderByDescending(p => p.CreatedAt).ToListAsync());
    }

    [Route("tin-tuc/{slug}")]
    public async Task<IActionResult> Detail(string slug)
    {
        var post = await _db.Posts.Include(p => p.PostCategory).FirstOrDefaultAsync(p => p.Slug == slug && p.IsActive);
        if (post == null) return NotFound();
        post.Views++;
        await _db.SaveChangesAsync();
        ViewBag.Others = await _db.Posts.Where(p => p.IsActive && p.Id != post.Id).OrderByDescending(p => p.CreatedAt).Take(3).ToListAsync();
        return View(post);
    }
}
