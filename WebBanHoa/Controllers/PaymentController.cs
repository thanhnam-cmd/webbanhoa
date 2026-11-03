using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanHoa.Data;
using WebBanHoa.Infrastructure;
using WebBanHoa.Models;
using WebBanHoa.Services;

namespace WebBanHoa.Controllers;

public class PaymentController : Controller
{
    private readonly AppDbContext _db;
    private readonly VnPayService _vnPay;
    public PaymentController(AppDbContext db, VnPayService vnPay) { _db = db; _vnPay = vnPay; }

    /// <summary>VNPay chuyển hướng khách hàng về đây sau khi thanh toán.</summary>
    [Route("payment/vnpay-return")]
    public async Task<IActionResult> VnPayReturn()
    {
        var (order, ok, message) = await ProcessResultAsync();
        ViewBag.Ok = ok;
        ViewBag.Message = message;
        return View("PaymentResult", order);
    }

    /// <summary>IPN: VNPay gọi server-to-server để xác nhận kết quả giao dịch.</summary>
    [Route("payment/vnpay-ipn")]
    public async Task<IActionResult> VnPayIpn()
    {
        if (!_vnPay.ValidateSignature(Request.Query)) return Json(new { RspCode = "97", Message = "Invalid signature" });
        var (order, _, _) = await ProcessResultAsync();
        return order == null ? Json(new { RspCode = "01", Message = "Order not found" }) : Json(new { RspCode = "00", Message = "Confirm Success" });
    }

    private async Task<(Order? order, bool ok, string message)> ProcessResultAsync()
    {
        var q = Request.Query;
        if (!_vnPay.ValidateSignature(q)) return (null, false, "Chữ ký không hợp lệ – dữ liệu thanh toán có thể đã bị thay đổi.");

        var txnRef = q["vnp_TxnRef"].ToString();
        var code = txnRef.Split('-')[0];
        var order = await _db.Orders.Include(o => o.Details).FirstOrDefaultAsync(o => o.Code == code);
        if (order == null) return (null, false, "Không tìm thấy đơn hàng.");

        var responseCode = q["vnp_ResponseCode"].ToString();
        var success = responseCode == "00";
        var amount = decimal.TryParse(q["vnp_Amount"], NumberStyles.Any, CultureInfo.InvariantCulture, out var a) ? a / 100 : 0;
        if (success && amount != order.Total) { success = false; responseCode = "04"; }

        // Chỉ ghi nhận giao dịch 1 lần cho mỗi mã tham chiếu
        if (!await _db.PaymentTransactions.AnyAsync(t => t.TxnRef == txnRef))
        {
            _db.PaymentTransactions.Add(new PaymentTransaction
            {
                OrderId = order.Id, Provider = "VNPAY", TxnRef = txnRef,
                TransactionNo = q["vnp_TransactionNo"], BankCode = q["vnp_BankCode"], Amount = amount,
                ResponseCode = responseCode, Message = VnPayService.ResponseMessage(responseCode), IsSuccess = success,
            });
            if (order.PaymentStatus != PaymentStatus.DaThanhToan)
                order.PaymentStatus = success ? PaymentStatus.DaThanhToan : PaymentStatus.ThatBai;
            order.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }
        return (order, order.PaymentStatus == PaymentStatus.DaThanhToan, VnPayService.ResponseMessage(responseCode));
    }

    // ================= Cổng thanh toán mô phỏng (sandbox nội bộ) =================
    // Dùng khi chưa cấu hình tài khoản merchant VNPay. Cổng nhận đúng bộ tham số vnp_* có chữ ký,
    // cho phép chọn ngân hàng, nhập thẻ test, OTP và trả kết quả có chữ ký về ReturnUrl giống VNPay thật.

    [HttpGet("payment/demo-gateway")]
    public IActionResult DemoGateway()
    {
        if (!_vnPay.ValidateSignature(Request.Query)) return BadRequest("Chữ ký yêu cầu thanh toán không hợp lệ.");
        ViewBag.Amount = decimal.Parse(Request.Query["vnp_Amount"]!, CultureInfo.InvariantCulture) / 100;
        ViewBag.OrderInfo = Request.Query["vnp_OrderInfo"].ToString();
        ViewBag.TxnRef = Request.Query["vnp_TxnRef"].ToString();
        ViewBag.Query = Request.QueryString.Value;
        return View();
    }

    [HttpPost("payment/demo-gateway"), ValidateAntiForgeryToken]
    public IActionResult DemoGatewayConfirm(string query, string bankCode, string decision)
    {
        var original = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(query);
        var fake = new QueryCollection(original);
        if (!_vnPay.ValidateSignature(fake)) return BadRequest("Chữ ký yêu cầu thanh toán không hợp lệ.");

        var ok = decision == "pay";
        var result = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["vnp_Amount"] = original["vnp_Amount"]!,
            ["vnp_BankCode"] = string.IsNullOrEmpty(bankCode) ? "NCB" : bankCode,
            ["vnp_CardType"] = "ATM",
            ["vnp_OrderInfo"] = original["vnp_OrderInfo"]!,
            ["vnp_PayDate"] = Helpers.VnNow.ToString("yyyyMMddHHmmss"),
            ["vnp_ResponseCode"] = ok ? "00" : "24",
            ["vnp_TmnCode"] = original["vnp_TmnCode"]!,
            ["vnp_TransactionNo"] = ok ? Random.Shared.Next(10000000, 99999999).ToString() : "0",
            ["vnp_TransactionStatus"] = ok ? "00" : "02",
            ["vnp_TxnRef"] = original["vnp_TxnRef"]!,
        };
        var url = Url.Action(nameof(VnPayReturn))! + "?" + VnPayService.BuildQuery(result) + "&vnp_SecureHash=" + _vnPay.Sign(result);
        return Redirect(url);
    }
}
