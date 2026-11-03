using WebBanHoa.Data;
using WebBanHoa.Models;

namespace WebBanHoa.Services;

/// <summary>Lưu ảnh tải lên vào bảng MediaFiles và trả về đường dẫn /media/{id}.</summary>
public class MediaService
{
    private static readonly string[] Allowed = { "image/jpeg", "image/png", "image/webp", "image/gif" };
    private readonly AppDbContext _db;
    public MediaService(AppDbContext db) => _db = db;

    public async Task<string?> SaveAsync(IFormFile? file)
    {
        if (file == null || file.Length == 0) return null;
        if (!Allowed.Contains(file.ContentType) || file.Length > 4 * 1024 * 1024)
            throw new InvalidOperationException("Chỉ chấp nhận ảnh JPG, PNG, WEBP, GIF dưới 4MB.");
        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        var m = new MediaFile { FileName = Path.GetFileName(file.FileName), ContentType = file.ContentType, Data = ms.ToArray() };
        _db.MediaFiles.Add(m);
        await _db.SaveChangesAsync();
        return $"/media/{m.Id}";
    }
}
