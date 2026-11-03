using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebBanHoa.Models;

/// <summary>Danh mục hoa (Hoa sinh nhật, Hoa cưới, Lẵng hoa khai trương...)</summary>
public class Category
{
    public int Id { get; set; }
    [Required(ErrorMessage = "Vui lòng nhập tên danh mục"), StringLength(100)]
    [Display(Name = "Tên danh mục")] public string Name { get; set; } = "";
    [StringLength(120)] public string Slug { get; set; } = "";
    [StringLength(300)][Display(Name = "Mô tả")] public string? Description { get; set; }
    [StringLength(300)][Display(Name = "Hình ảnh")] public string? Image { get; set; }
    [Display(Name = "Thứ tự")] public int SortOrder { get; set; }
    [Display(Name = "Hiển thị")] public bool IsActive { get; set; } = true;
    public List<Product> Products { get; set; } = new();
}

/// <summary>Dịp tặng hoa (Sinh nhật, Tình yêu, Chúc mừng, Chia buồn...) – tương đương "thương hiệu".</summary>
public class Occasion
{
    public int Id { get; set; }
    [Required(ErrorMessage = "Vui lòng nhập tên dịp tặng"), StringLength(100)]
    [Display(Name = "Tên dịp tặng")] public string Name { get; set; } = "";
    [StringLength(120)] public string Slug { get; set; } = "";
    [StringLength(300)][Display(Name = "Mô tả")] public string? Description { get; set; }
    [Display(Name = "Hiển thị")] public bool IsActive { get; set; } = true;
    public List<Product> Products { get; set; } = new();
}

public class Product
{
    public int Id { get; set; }
    [Required(ErrorMessage = "Vui lòng nhập tên sản phẩm"), StringLength(150)]
    [Display(Name = "Tên sản phẩm")] public string Name { get; set; } = "";
    [StringLength(170)] public string Slug { get; set; } = "";
    [StringLength(30)][Display(Name = "Mã SP")] public string? Sku { get; set; }
    [Display(Name = "Danh mục")] public int CategoryId { get; set; }
    public Category? Category { get; set; }
    [Display(Name = "Dịp tặng")] public int? OccasionId { get; set; }
    public Occasion? Occasion { get; set; }
    [Range(0, 1_000_000_000, ErrorMessage = "Giá không hợp lệ")]
    [Column(TypeName = "decimal(18,0)")][Display(Name = "Giá gốc")] public decimal Price { get; set; }
    [Column(TypeName = "decimal(18,0)")][Display(Name = "Giá khuyến mãi")] public decimal? SalePrice { get; set; }
    [StringLength(300)][Display(Name = "Hình ảnh")] public string? Image { get; set; }
    [StringLength(400)][Display(Name = "Mô tả ngắn")] public string? ShortDescription { get; set; }
    [Display(Name = "Mô tả chi tiết")] public string? Description { get; set; }
    [StringLength(400)][Display(Name = "Ý nghĩa")] public string? Meaning { get; set; }
    [Display(Name = "Tồn kho")] public int Stock { get; set; } = 50;
    [Display(Name = "Đã bán")] public int Sold { get; set; }
    [Display(Name = "Nổi bật")] public bool IsFeatured { get; set; }
    [Display(Name = "Hiển thị")] public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [NotMapped] public decimal FinalPrice => SalePrice is > 0 && SalePrice < Price ? SalePrice.Value : Price;
    [NotMapped] public int DiscountPercent => SalePrice is > 0 && SalePrice < Price ? (int)Math.Round((1 - SalePrice.Value / Price) * 100) : 0;
}

public class Slider
{
    public int Id { get; set; }
    [Required(ErrorMessage = "Vui lòng nhập tiêu đề"), StringLength(150)]
    [Display(Name = "Tiêu đề")] public string Title { get; set; } = "";
    [StringLength(300)][Display(Name = "Mô tả")] public string? Subtitle { get; set; }
    [StringLength(300)][Display(Name = "Hình ảnh")] public string? Image { get; set; }
    [StringLength(300)][Display(Name = "Đường dẫn")] public string? Link { get; set; }
    [Display(Name = "Thứ tự")] public int SortOrder { get; set; }
    [Display(Name = "Hiển thị")] public bool IsActive { get; set; } = true;
}

public class PostCategory
{
    public int Id { get; set; }
    [Required(ErrorMessage = "Vui lòng nhập tên danh mục tin"), StringLength(100)]
    [Display(Name = "Tên danh mục tin")] public string Name { get; set; } = "";
    [StringLength(120)] public string Slug { get; set; } = "";
    [Display(Name = "Hiển thị")] public bool IsActive { get; set; } = true;
    public List<Post> Posts { get; set; } = new();
}

public class Post
{
    public int Id { get; set; }
    [Required(ErrorMessage = "Vui lòng nhập tiêu đề"), StringLength(200)]
    [Display(Name = "Tiêu đề")] public string Title { get; set; } = "";
    [StringLength(220)] public string Slug { get; set; } = "";
    [StringLength(500)][Display(Name = "Tóm tắt")] public string? Summary { get; set; }
    [Display(Name = "Nội dung")] public string? Content { get; set; }
    [StringLength(300)][Display(Name = "Hình ảnh")] public string? Image { get; set; }
    [Display(Name = "Danh mục tin")] public int PostCategoryId { get; set; }
    public PostCategory? PostCategory { get; set; }
    [Display(Name = "Hiển thị")] public bool IsActive { get; set; } = true;
    public int Views { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Customer
{
    public int Id { get; set; }
    [Required, StringLength(100)] public string FullName { get; set; } = "";
    [Required, StringLength(150)] public string Email { get; set; } = "";
    [StringLength(20)] public string? Phone { get; set; }
    [StringLength(300)] public string? Address { get; set; }
    [StringLength(200)] public string PasswordHash { get; set; } = "";
    public bool IsLocked { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<Order> Orders { get; set; } = new();
}

public class AdminUser
{
    public int Id { get; set; }
    [Required, StringLength(50)] public string Username { get; set; } = "";
    [StringLength(100)] public string FullName { get; set; } = "";
    [StringLength(200)] public string PasswordHash { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public enum OrderStatus { ChoXacNhan = 0, DaXacNhan = 1, DangGiao = 2, HoanThanh = 3, DaHuy = 4 }
public enum PaymentMethod { COD = 0, VNPay = 1 }
public enum PaymentStatus { ChuaThanhToan = 0, DaThanhToan = 1, ThatBai = 2 }

public class Order
{
    public int Id { get; set; }
    [StringLength(20)] public string Code { get; set; } = "";
    public int? CustomerId { get; set; }
    public Customer? Customer { get; set; }

    // Người gửi (người đặt hoa)
    [StringLength(100)] public string SenderName { get; set; } = "";
    [StringLength(20)] public string SenderPhone { get; set; } = "";
    [StringLength(150)] public string? SenderEmail { get; set; }

    // Người nhận (giao hàng – tương đương bảng shipping)
    [StringLength(100)] public string ReceiverName { get; set; } = "";
    [StringLength(20)] public string ReceiverPhone { get; set; } = "";
    [StringLength(300)] public string Address { get; set; } = "";
    public DateTime DeliveryDate { get; set; }
    [StringLength(30)] public string DeliverySlot { get; set; } = "";
    [StringLength(300)] public string? CardMessage { get; set; }
    [StringLength(500)] public string? Note { get; set; }

    [Column(TypeName = "decimal(18,0)")] public decimal Subtotal { get; set; }
    [Column(TypeName = "decimal(18,0)")] public decimal ShippingFee { get; set; }
    [Column(TypeName = "decimal(18,0)")] public decimal Total { get; set; }

    public PaymentMethod PaymentMethod { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public OrderStatus Status { get; set; }
    [StringLength(300)] public string? CancelReason { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public List<OrderDetail> Details { get; set; } = new();
    public List<PaymentTransaction> Transactions { get; set; } = new();
}

public class OrderDetail
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public Order? Order { get; set; }
    public int? ProductId { get; set; }
    public Product? Product { get; set; }
    [StringLength(150)] public string ProductName { get; set; } = "";
    [StringLength(300)] public string? Image { get; set; }
    [Column(TypeName = "decimal(18,0)")] public decimal Price { get; set; }
    public int Quantity { get; set; }
}

/// <summary>Lưu lại mỗi giao dịch thanh toán online (tương đương bảng "momo" trong đồ án mẫu).</summary>
public class PaymentTransaction
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public Order? Order { get; set; }
    [StringLength(30)] public string Provider { get; set; } = "";
    [StringLength(50)] public string TxnRef { get; set; } = "";
    [StringLength(50)] public string? TransactionNo { get; set; }
    [Column(TypeName = "decimal(18,0)")] public decimal Amount { get; set; }
    [StringLength(20)] public string? BankCode { get; set; }
    [StringLength(10)] public string? ResponseCode { get; set; }
    [StringLength(300)] public string? Message { get; set; }
    public bool IsSuccess { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class ContactMessage
{
    public int Id { get; set; }
    [Required(ErrorMessage = "Vui lòng nhập họ tên"), StringLength(100)] public string FullName { get; set; } = "";
    [Required(ErrorMessage = "Vui lòng nhập email"), EmailAddress(ErrorMessage = "Email không hợp lệ"), StringLength(150)] public string Email { get; set; } = "";
    [StringLength(20)] public string? Phone { get; set; }
    [Required(ErrorMessage = "Vui lòng nhập nội dung"), StringLength(2000)] public string Message { get; set; } = "";
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>Ảnh do quản trị viên tải lên, lưu thẳng vào CSDL để không mất khi deploy lại.</summary>
public class MediaFile
{
    public int Id { get; set; }
    [StringLength(200)] public string FileName { get; set; } = "";
    [StringLength(100)] public string ContentType { get; set; } = "";
    public byte[] Data { get; set; } = Array.Empty<byte>();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
