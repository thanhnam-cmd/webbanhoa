using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebBanHoa.Data;
using WebBanHoa.Infrastructure;
using WebBanHoa.Models;
using WebBanHoa.Services;

namespace WebBanHoa.Areas.Admin.Controllers;

// ======================= Danh mục sản phẩm =======================
public class CategoriesController : AdminBaseController
{
    private readonly MediaService _media;
    public CategoriesController(AppDbContext db, MediaService media) : base(db) => _media = media;

    public async Task<IActionResult> Index(string? q, int page = 1)
    {
        var query = Db.Categories.Include(c => c.Products).AsQueryable();
        if (!string.IsNullOrWhiteSpace(q)) query = query.Where(c => c.Name.Contains(q));
        return View(await PageAsync(query.OrderBy(c => c.SortOrder).ThenBy(c => c.Name), page, q));
    }

    public IActionResult Create() => View("Form", new Category());

    public async Task<IActionResult> Edit(int id)
    {
        var c = await Db.Categories.FindAsync(id);
        return c == null ? NotFound() : View("Form", c);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(Category model, IFormFile? upload)
    {
        model.Image = await ResolveImageAsync(upload, model.Image, _media);
        if (!ModelState.IsValid) return View("Form", model);
        model.Slug = await UniqueSlugAsync(Db.Categories, model.Name.ToSlug(), s => c => c.Slug == s && c.Id != model.Id);
        if (model.Id == 0) Db.Categories.Add(model); else Db.Categories.Update(model);
        await Db.SaveChangesAsync();
        Toast(model.Id == 0 ? "Đã thêm danh mục" : "Đã lưu danh mục “" + model.Name + "”");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var c = await Db.Categories.Include(x => x.Products).FirstOrDefaultAsync(x => x.Id == id);
        if (c == null) return NotFound();
        if (c.Products.Count > 0) Toast($"Không thể xóa: danh mục đang có {c.Products.Count} sản phẩm", "danger");
        else { Db.Categories.Remove(c); await Db.SaveChangesAsync(); Toast("Đã xóa danh mục"); }
        return RedirectToAction(nameof(Index));
    }
}

// ======================= Dịp tặng (tương đương thương hiệu) =======================
public class OccasionsController : AdminBaseController
{
    public OccasionsController(AppDbContext db) : base(db) { }

    public async Task<IActionResult> Index(string? q, int page = 1)
    {
        var query = Db.Occasions.Include(o => o.Products).AsQueryable();
        if (!string.IsNullOrWhiteSpace(q)) query = query.Where(o => o.Name.Contains(q));
        return View(await PageAsync(query.OrderBy(o => o.Id), page, q));
    }

    public IActionResult Create() => View("Form", new Occasion());

    public async Task<IActionResult> Edit(int id)
    {
        var o = await Db.Occasions.FindAsync(id);
        return o == null ? NotFound() : View("Form", o);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(Occasion model)
    {
        if (!ModelState.IsValid) return View("Form", model);
        model.Slug = await UniqueSlugAsync(Db.Occasions, model.Name.ToSlug(), s => o => o.Slug == s && o.Id != model.Id);
        if (model.Id == 0) Db.Occasions.Add(model); else Db.Occasions.Update(model);
        await Db.SaveChangesAsync();
        Toast("Đã lưu dịp tặng “" + model.Name + "”");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var o = await Db.Occasions.FindAsync(id);
        if (o == null) return NotFound();
        Db.Occasions.Remove(o);
        await Db.SaveChangesAsync();
        Toast("Đã xóa dịp tặng");
        return RedirectToAction(nameof(Index));
    }
}

// ======================= Sản phẩm =======================
public class ProductsController : AdminBaseController
{
    private readonly MediaService _media;
    public ProductsController(AppDbContext db, MediaService media) : base(db) => _media = media;

    public async Task<IActionResult> Index(string? q, int? categoryId, int page = 1)
    {
        var query = Db.Products.Include(p => p.Category).Include(p => p.Occasion).AsQueryable();
        if (!string.IsNullOrWhiteSpace(q)) query = query.Where(p => p.Name.Contains(q) || (p.Sku != null && p.Sku.Contains(q)));
        if (categoryId.HasValue) query = query.Where(p => p.CategoryId == categoryId);
        ViewBag.Categories = new SelectList(await Db.Categories.OrderBy(c => c.SortOrder).ToListAsync(), "Id", "Name", categoryId);
        ViewBag.CategoryId = categoryId;
        return View(await PageAsync(query.OrderByDescending(p => p.UpdatedAt), page, q));
    }

    public async Task<IActionResult> Create()
    {
        await LoadLookups();
        return View("Form", new Product { Stock = 30 });
    }

    public async Task<IActionResult> Edit(int id)
    {
        var p = await Db.Products.FindAsync(id);
        if (p == null) return NotFound();
        await LoadLookups();
        return View("Form", p);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(Product model, IFormFile? upload)
    {
        model.Image = await ResolveImageAsync(upload, model.Image, _media);
        if (model.SalePrice is <= 0) model.SalePrice = null;
        if (model.SalePrice >= model.Price) ModelState.AddModelError(nameof(model.SalePrice), "Giá khuyến mãi phải nhỏ hơn giá gốc");
        if (!ModelState.IsValid) { await LoadLookups(); return View("Form", model); }

        model.Slug = await UniqueSlugAsync(Db.Products, model.Name.ToSlug(), s => p => p.Slug == s && p.Id != model.Id);
        model.UpdatedAt = DateTime.UtcNow;
        if (model.Id == 0)
        {
            model.CreatedAt = DateTime.UtcNow;
            Db.Products.Add(model);
            await Db.SaveChangesAsync();
            if (string.IsNullOrEmpty(model.Sku)) { model.Sku = $"FL{model.Id:000}"; await Db.SaveChangesAsync(); }
        }
        else
        {
            var existing = await Db.Products.AsNoTracking().FirstAsync(p => p.Id == model.Id);
            model.CreatedAt = existing.CreatedAt;
            model.Sold = existing.Sold;
            Db.Products.Update(model);
            await Db.SaveChangesAsync();
        }
        Toast("Đã lưu sản phẩm “" + model.Name + "”");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(int id)
    {
        var p = await Db.Products.FindAsync(id);
        if (p == null) return NotFound();
        p.IsActive = !p.IsActive;
        await Db.SaveChangesAsync();
        return Json(new { ok = true, active = p.IsActive });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var p = await Db.Products.FindAsync(id);
        if (p == null) return NotFound();
        Db.Products.Remove(p); // chi tiết đơn hàng cũ vẫn giữ tên & giá (ProductId = NULL)
        await Db.SaveChangesAsync();
        Toast("Đã xóa sản phẩm");
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadLookups()
    {
        ViewBag.Categories = new SelectList(await Db.Categories.OrderBy(c => c.SortOrder).ToListAsync(), "Id", "Name");
        ViewBag.Occasions = new SelectList(await Db.Occasions.ToListAsync(), "Id", "Name");
    }
}

// ======================= Slide =======================
public class SlidersController : AdminBaseController
{
    private readonly MediaService _media;
    public SlidersController(AppDbContext db, MediaService media) : base(db) => _media = media;

    public async Task<IActionResult> Index() => View(await Db.Sliders.OrderBy(s => s.SortOrder).ToListAsync());

    public IActionResult Create() => View("Form", new Slider());

    public async Task<IActionResult> Edit(int id)
    {
        var s = await Db.Sliders.FindAsync(id);
        return s == null ? NotFound() : View("Form", s);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(Slider model, IFormFile? upload)
    {
        model.Image = await ResolveImageAsync(upload, model.Image, _media);
        if (!ModelState.IsValid) return View("Form", model);
        if (model.Id == 0) Db.Sliders.Add(model); else Db.Sliders.Update(model);
        await Db.SaveChangesAsync();
        Toast("Đã lưu slide");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var s = await Db.Sliders.FindAsync(id);
        if (s == null) return NotFound();
        Db.Sliders.Remove(s);
        await Db.SaveChangesAsync();
        Toast("Đã xóa slide");
        return RedirectToAction(nameof(Index));
    }
}

// ======================= Danh mục tin tức =======================
public class PostCategoriesController : AdminBaseController
{
    public PostCategoriesController(AppDbContext db) : base(db) { }

    public async Task<IActionResult> Index() => View(await Db.PostCategories.Include(c => c.Posts).OrderBy(c => c.Id).ToListAsync());

    public IActionResult Create() => View("Form", new PostCategory());

    public async Task<IActionResult> Edit(int id)
    {
        var c = await Db.PostCategories.FindAsync(id);
        return c == null ? NotFound() : View("Form", c);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(PostCategory model)
    {
        if (!ModelState.IsValid) return View("Form", model);
        model.Slug = await UniqueSlugAsync(Db.PostCategories, model.Name.ToSlug(), s => c => c.Slug == s && c.Id != model.Id);
        if (model.Id == 0) Db.PostCategories.Add(model); else Db.PostCategories.Update(model);
        await Db.SaveChangesAsync();
        Toast("Đã lưu danh mục tin");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var c = await Db.PostCategories.Include(x => x.Posts).FirstOrDefaultAsync(x => x.Id == id);
        if (c == null) return NotFound();
        if (c.Posts.Count > 0) Toast($"Không thể xóa: danh mục đang có {c.Posts.Count} bài viết", "danger");
        else { Db.PostCategories.Remove(c); await Db.SaveChangesAsync(); Toast("Đã xóa danh mục tin"); }
        return RedirectToAction(nameof(Index));
    }
}

// ======================= Tin tức =======================
public class PostsController : AdminBaseController
{
    private readonly MediaService _media;
    public PostsController(AppDbContext db, MediaService media) : base(db) => _media = media;

    public async Task<IActionResult> Index(string? q, int page = 1)
    {
        var query = Db.Posts.Include(p => p.PostCategory).AsQueryable();
        if (!string.IsNullOrWhiteSpace(q)) query = query.Where(p => p.Title.Contains(q));
        return View(await PageAsync(query.OrderByDescending(p => p.CreatedAt), page, q));
    }

    public async Task<IActionResult> Create()
    {
        await LoadLookups();
        return View("Form", new Post());
    }

    public async Task<IActionResult> Edit(int id)
    {
        var p = await Db.Posts.FindAsync(id);
        if (p == null) return NotFound();
        await LoadLookups();
        return View("Form", p);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(Post model, IFormFile? upload)
    {
        model.Image = await ResolveImageAsync(upload, model.Image, _media);
        if (!ModelState.IsValid) { await LoadLookups(); return View("Form", model); }
        model.Slug = await UniqueSlugAsync(Db.Posts, model.Title.ToSlug(), s => p => p.Slug == s && p.Id != model.Id);
        if (model.Id == 0) { model.CreatedAt = DateTime.UtcNow; Db.Posts.Add(model); }
        else
        {
            var existing = await Db.Posts.AsNoTracking().FirstAsync(p => p.Id == model.Id);
            model.CreatedAt = existing.CreatedAt; model.Views = existing.Views;
            Db.Posts.Update(model);
        }
        await Db.SaveChangesAsync();
        Toast("Đã lưu bài viết");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var p = await Db.Posts.FindAsync(id);
        if (p == null) return NotFound();
        Db.Posts.Remove(p);
        await Db.SaveChangesAsync();
        Toast("Đã xóa bài viết");
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadLookups() =>
        ViewBag.PostCategories = new SelectList(await Db.PostCategories.ToListAsync(), "Id", "Name");
}
