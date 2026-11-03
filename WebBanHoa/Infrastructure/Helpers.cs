using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using WebBanHoa.Models;

namespace WebBanHoa.Infrastructure;

public static class Helpers
{
    private static readonly CultureInfo Vi = new("vi-VN");

    public static string ToVnd(this decimal value) => value.ToString("#,##0", Vi) + "₫";

    /// <summary>"Hoa Hồng Đỏ" → "hoa-hong-do"</summary>
    public static string ToSlug(this string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return "";
        var s = text.Trim().ToLowerInvariant().Replace('đ', 'd');
        var normalized = s.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (var c in normalized)
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark) sb.Append(c);
        s = Regex.Replace(sb.ToString(), @"[^a-z0-9\s-]", "");
        s = Regex.Replace(s, @"[\s-]+", "-").Trim('-');
        return s;
    }

    public static string ToLocal(this DateTime utc) =>
        TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), VnTimeZone).ToString("dd/MM/yyyy HH:mm");

    public static DateTime VnNow => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, VnTimeZone);

    private static TimeZoneInfo? _tz;
    private static TimeZoneInfo VnTimeZone => _tz ??= FindTz();
    private static TimeZoneInfo FindTz()
    {
        foreach (var id in new[] { "Asia/Ho_Chi_Minh", "SE Asia Standard Time" })
            try { return TimeZoneInfo.FindSystemTimeZoneById(id); } catch { }
        return TimeZoneInfo.CreateCustomTimeZone("VN", TimeSpan.FromHours(7), "VN", "VN");
    }

    public static string Label(this OrderStatus s) => s switch
    {
        OrderStatus.ChoXacNhan => "Chờ xác nhận",
        OrderStatus.DaXacNhan => "Đã xác nhận",
        OrderStatus.DangGiao => "Đang giao",
        OrderStatus.HoanThanh => "Hoàn thành",
        OrderStatus.DaHuy => "Đã hủy",
        _ => s.ToString()
    };

    public static string Css(this OrderStatus s) => s switch
    {
        OrderStatus.ChoXacNhan => "warn",
        OrderStatus.DaXacNhan => "info",
        OrderStatus.DangGiao => "violet",
        OrderStatus.HoanThanh => "ok",
        _ => "danger"
    };

    public static string Label(this PaymentStatus s) => s switch
    {
        PaymentStatus.DaThanhToan => "Đã thanh toán",
        PaymentStatus.ThatBai => "Thanh toán lỗi",
        _ => "Chưa thanh toán"
    };

    public static string Css(this PaymentStatus s) => s switch
    {
        PaymentStatus.DaThanhToan => "ok",
        PaymentStatus.ThatBai => "danger",
        _ => "muted"
    };

    public static string Label(this PaymentMethod m) => m == PaymentMethod.VNPay ? "Thanh toán online (VNPay)" : "Thanh toán khi nhận hàng (COD)";

    public static string QueryQ(string? q) => string.IsNullOrEmpty(q) ? "" : "q=" + Uri.EscapeDataString(q);

    public static string ImageOrDefault(this string? url) => string.IsNullOrWhiteSpace(url) ? "/img/flowers/placeholder.svg" : url;

    public static readonly string[] DeliverySlots = { "08:00 - 10:00", "10:00 - 12:00", "13:00 - 15:00", "15:00 - 17:00", "17:00 - 19:00", "19:00 - 21:00" };
}
