using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanHoa.Data;
using WebBanHoa.Models;

namespace WebBanHoa.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext _db;
    public HomeController(AppDbContext db) => _db = db;

    [Route("")]
    public async Task<IActionResult> Index()
    {
        var active = _db.Products.Where(p => p.IsActive);
        var vm = new HomeViewModel
        {
            Sliders = await _db.Sliders.Where(s => s.IsActive).OrderBy(s => s.SortOrder).ToListAsync(),
            Categories = await _db.Categories.Where(c => c.IsActive).OrderBy(c => c.SortOrder).ToListAsync(),
            Occasions = await _db.Occasions.Where(o => o.IsActive).ToListAsync(),
            Featured = await active.Where(p => p.IsFeatured).OrderByDescending(p => p.CreatedAt).Take(8).ToListAsync(),
            BestSellers = await active.OrderByDescending(p => p.Sold).Take(4).ToListAsync(),
            OnSale = await active.Where(p => p.SalePrice != null && p.SalePrice < p.Price).OrderByDescending(p => p.Price - p.SalePrice).Take(4).ToListAsync(),
            Posts = await _db.Posts.Where(p => p.IsActive).OrderByDescending(p => p.CreatedAt).Take(3).ToListAsync(),
        };
        return View(vm);
    }

    [Route("gioi-thieu")]
    public IActionResult About() => View();

    [HttpGet("lien-he")]
    public IActionResult Contact() => View(new ContactMessage());

    [HttpPost("lien-he"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Contact(ContactMessage model)
    {
        if (!ModelState.IsValid) return View(model);
        model.Id = 0; model.IsRead = false; model.CreatedAt = DateTime.UtcNow;
        _db.ContactMessages.Add(model);
        await _db.SaveChangesAsync();
        TempData["Toast"] = "Cảm ơn bạn! Floria sẽ liên hệ lại trong thời gian sớm nhất.";
        return RedirectToAction(nameof(Contact));
    }

    /// <summary>Trả về ảnh đã tải lên (lưu trong CSDL).</summary>
    [Route("media/{id:int}")]
    [ResponseCache(Duration = 86400)]
    public async Task<IActionResult> Media(int id)
    {
        var m = await _db.MediaFiles.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        return m == null ? NotFound() : File(m.Data, m.ContentType);
    }

    public IActionResult Error() => View();

    public IActionResult NotFound404()
    {
        Response.StatusCode = 404;
        return View();
    }
}
