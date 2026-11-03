using System.ComponentModel.DataAnnotations;
using WebBanHoa.Services;

namespace WebBanHoa.Models;

public class HomeViewModel
{
    public List<Slider> Sliders { get; set; } = new();
    public List<Category> Categories { get; set; } = new();
    public List<Occasion> Occasions { get; set; } = new();
    public List<Product> Featured { get; set; } = new();
    public List<Product> BestSellers { get; set; } = new();
    public List<Product> OnSale { get; set; } = new();
    public List<Post> Posts { get; set; } = new();
}

public class ShopViewModel
{
    public List<Product> Products { get; set; } = new();
    public List<Category> Categories { get; set; } = new();
    public List<Occasion> Occasions { get; set; } = new();
    public string? Q { get; set; }
    public string? Category { get; set; }
    public string? Occasion { get; set; }
    public decimal? Min { get; set; }
    public decimal? Max { get; set; }
    public string Sort { get; set; } = "moi-nhat";
    public int Page { get; set; } = 1;
    public int TotalPages { get; set; }
    public int Total { get; set; }
    public string Title { get; set; } = "Tất cả sản phẩm";
}

public class ProductDetailViewModel
{
    public Product Product { get; set; } = null!;
    public List<Product> Related { get; set; } = new();
}

public class CheckoutViewModel
{
    public CartView? Cart { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập họ tên người gửi"), StringLength(100)]
    [Display(Name = "Họ tên người gửi")] public string SenderName { get; set; } = "";
    [Required(ErrorMessage = "Vui lòng nhập số điện thoại người gửi")]
    [RegularExpression(@"^(0|\+84)\d{9,10}$", ErrorMessage = "Số điện thoại không hợp lệ")]
    [Display(Name = "Số điện thoại")] public string SenderPhone { get; set; } = "";
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    [Display(Name = "Email")] public string? SenderEmail { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập tên người nhận"), StringLength(100)]
    [Display(Name = "Họ tên người nhận")] public string ReceiverName { get; set; } = "";
    [Required(ErrorMessage = "Vui lòng nhập số điện thoại người nhận")]
    [RegularExpression(@"^(0|\+84)\d{9,10}$", ErrorMessage = "Số điện thoại không hợp lệ")]
    [Display(Name = "Số điện thoại người nhận")] public string ReceiverPhone { get; set; } = "";
    [Required(ErrorMessage = "Vui lòng nhập địa chỉ giao hoa"), StringLength(300)]
    [Display(Name = "Địa chỉ giao hoa")] public string Address { get; set; } = "";
    [Required(ErrorMessage = "Vui lòng chọn ngày giao")]
    [DataType(DataType.Date)][Display(Name = "Ngày giao")] public DateTime DeliveryDate { get; set; }
    [Required(ErrorMessage = "Vui lòng chọn khung giờ")]
    [Display(Name = "Khung giờ")] public string DeliverySlot { get; set; } = "";
    [StringLength(300)][Display(Name = "Lời nhắn trên thiệp")] public string? CardMessage { get; set; }
    [StringLength(500)][Display(Name = "Ghi chú")] public string? Note { get; set; }
    [Display(Name = "Phương thức thanh toán")] public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.VNPay;
}

public class LoginViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập email"), EmailAddress(ErrorMessage = "Email không hợp lệ")]
    public string Email { get; set; } = "";
    [Required(ErrorMessage = "Vui lòng nhập mật khẩu"), DataType(DataType.Password)]
    public string Password { get; set; } = "";
    public bool RememberMe { get; set; } = true;
    public string? ReturnUrl { get; set; }
}

public class RegisterViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập họ tên"), StringLength(100)]
    public string FullName { get; set; } = "";
    [Required(ErrorMessage = "Vui lòng nhập email"), EmailAddress(ErrorMessage = "Email không hợp lệ")]
    public string Email { get; set; } = "";
    [RegularExpression(@"^(0|\+84)\d{9,10}$", ErrorMessage = "Số điện thoại không hợp lệ")]
    public string? Phone { get; set; }
    [Required(ErrorMessage = "Vui lòng nhập mật khẩu"), MinLength(6, ErrorMessage = "Mật khẩu tối thiểu 6 ký tự"), DataType(DataType.Password)]
    public string Password { get; set; } = "";
    [Compare(nameof(Password), ErrorMessage = "Mật khẩu xác nhận không khớp"), DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = "";
}

public class ProfileViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập họ tên"), StringLength(100)]
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    [RegularExpression(@"^(0|\+84)\d{9,10}$", ErrorMessage = "Số điện thoại không hợp lệ")]
    public string? Phone { get; set; }
    [StringLength(300)] public string? Address { get; set; }
    [DataType(DataType.Password)] public string? CurrentPassword { get; set; }
    [MinLength(6, ErrorMessage = "Mật khẩu tối thiểu 6 ký tự"), DataType(DataType.Password)] public string? NewPassword { get; set; }
}

public class AdminLoginViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")] public string Username { get; set; } = "";
    [Required(ErrorMessage = "Vui lòng nhập mật khẩu"), DataType(DataType.Password)] public string Password { get; set; } = "";
    public string? ReturnUrl { get; set; }
}

public class PagedList<T>
{
    public List<T> Items { get; set; } = new();
    public int Page { get; set; }
    public int TotalPages { get; set; }
    public int Total { get; set; }
    public string? Q { get; set; }
}
