using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using WebBanHoa.Infrastructure;

namespace WebBanHoa.Services;

public class VnPayOptions
{
    public string TmnCode { get; set; } = "";
    public string HashSecret { get; set; } = "";
    public string BaseUrl { get; set; } = "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";
    /// <summary>Khi chưa cấu hình TmnCode/HashSecret, hệ thống dùng cổng thanh toán mô phỏng nội bộ.</summary>
    public bool IsConfigured => !string.IsNullOrWhiteSpace(TmnCode) && !string.IsNullOrWhiteSpace(HashSecret);
}

/// <summary>
/// Tích hợp cổng thanh toán VNPay (phiên bản 2.1.0).
/// Quy trình: tạo URL thanh toán có chữ ký HMAC-SHA512 → khách thanh toán trên VNPay →
/// VNPay chuyển hướng về ReturnUrl kèm vnp_SecureHash → hệ thống kiểm tra chữ ký và cập nhật đơn hàng.
/// </summary>
public class VnPayService
{
    private readonly VnPayOptions _opt;
    // Khóa dùng cho cổng mô phỏng khi chưa có tài khoản merchant VNPay sandbox.
    private readonly string _demoSecret;

    public VnPayService(VnPayOptions opt, IConfiguration config)
    {
        _opt = opt;
        _demoSecret = config["DemoPay:Secret"] ?? "FLORIA-DEMO-SECRET-KEY";
    }

    public bool IsLive => _opt.IsConfigured;
    private string Secret => IsLive ? _opt.HashSecret : _demoSecret;

    public string CreatePaymentUrl(string orderCode, decimal amount, string orderInfo, string returnUrl, string ipAddress, string demoGatewayUrl)
    {
        var now = Helpers.VnNow;
        var data = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["vnp_Version"] = "2.1.0",
            ["vnp_Command"] = "pay",
            ["vnp_TmnCode"] = IsLive ? _opt.TmnCode : "FLORIADEMO",
            ["vnp_Amount"] = ((long)(amount * 100)).ToString(CultureInfo.InvariantCulture),
            ["vnp_CreateDate"] = now.ToString("yyyyMMddHHmmss"),
            ["vnp_ExpireDate"] = now.AddMinutes(15).ToString("yyyyMMddHHmmss"),
            ["vnp_CurrCode"] = "VND",
            ["vnp_IpAddr"] = string.IsNullOrEmpty(ipAddress) ? "127.0.0.1" : ipAddress,
            ["vnp_Locale"] = "vn",
            ["vnp_OrderInfo"] = orderInfo,
            ["vnp_OrderType"] = "other",
            ["vnp_ReturnUrl"] = returnUrl,
            ["vnp_TxnRef"] = orderCode + "-" + now.ToString("HHmmss"),
        };
        var query = BuildQuery(data);
        var hash = HmacSha512(Secret, query);
        var baseUrl = IsLive ? _opt.BaseUrl : demoGatewayUrl;
        return $"{baseUrl}?{query}&vnp_SecureHash={hash}";
    }

    /// <summary>Kiểm tra chữ ký dữ liệu VNPay trả về.</summary>
    public bool ValidateSignature(IQueryCollection query)
    {
        var received = query["vnp_SecureHash"].ToString();
        var data = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var (k, v) in query)
            if (k.StartsWith("vnp_") && k != "vnp_SecureHash" && k != "vnp_SecureHashType" && !string.IsNullOrEmpty(v))
                data[k] = v.ToString();
        var hash = HmacSha512(Secret, BuildQuery(data));
        return string.Equals(hash, received, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Ký lại bộ tham số kết quả (dùng cho cổng mô phỏng).</summary>
    public string Sign(SortedDictionary<string, string> data) => HmacSha512(Secret, BuildQuery(data));

    public static string BuildQuery(SortedDictionary<string, string> data) =>
        string.Join("&", data.Where(kv => !string.IsNullOrEmpty(kv.Value))
            .Select(kv => WebUtility.UrlEncode(kv.Key) + "=" + WebUtility.UrlEncode(kv.Value)));

    private static string HmacSha512(string key, string input)
    {
        using var h = new HMACSHA512(Encoding.UTF8.GetBytes(key));
        return Convert.ToHexString(h.ComputeHash(Encoding.UTF8.GetBytes(input))).ToLowerInvariant();
    }

    public static string ResponseMessage(string code) => code switch
    {
        "00" => "Giao dịch thành công",
        "07" => "Trừ tiền thành công, giao dịch bị nghi ngờ",
        "09" => "Thẻ/Tài khoản chưa đăng ký Internet Banking",
        "10" => "Xác thực thông tin thẻ/tài khoản không đúng quá 3 lần",
        "11" => "Đã hết hạn chờ thanh toán",
        "12" => "Thẻ/Tài khoản bị khóa",
        "13" => "Nhập sai mật khẩu xác thực giao dịch (OTP)",
        "24" => "Khách hàng hủy giao dịch",
        "51" => "Tài khoản không đủ số dư",
        "65" => "Tài khoản đã vượt quá hạn mức giao dịch trong ngày",
        "75" => "Ngân hàng thanh toán đang bảo trì",
        _ => "Giao dịch không thành công"
    };
}
