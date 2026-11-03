using Microsoft.EntityFrameworkCore;
using WebBanHoa.Infrastructure;
using WebBanHoa.Models;

namespace WebBanHoa.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        await db.Database.EnsureCreatedAsync();
        if (await db.AdminUsers.AnyAsync()) return;

        db.AdminUsers.Add(new AdminUser { Username = "admin", FullName = "Quản trị viên", PasswordHash = Passwords.Hash("Admin@123") });

        var cats = new[]
        {
            new Category { Name = "Bó hoa", Description = "Những bó hoa tươi gói tay tinh tế cho mọi dịp", Image = "/img/flowers/p01.svg", SortOrder = 1 },
            new Category { Name = "Hộp hoa", Description = "Hoa cắm hộp sang trọng, giữ dáng lâu", Image = "/img/flowers/p05.svg", SortOrder = 2 },
            new Category { Name = "Giỏ hoa", Description = "Giỏ hoa mây tre đan mộc mạc, ấm áp", Image = "/img/flowers/p09.svg", SortOrder = 3 },
            new Category { Name = "Kệ hoa khai trương", Description = "Kệ hoa chúc mừng khai trương hồng phát", Image = "/img/flowers/p12.svg", SortOrder = 4 },
            new Category { Name = "Hoa cưới", Description = "Hoa cầm tay cô dâu cho ngày trọng đại", Image = "/img/flowers/p15.svg", SortOrder = 5 },
            new Category { Name = "Lan hồ điệp", Description = "Chậu lan hồ điệp quý phái, bền lâu", Image = "/img/flowers/p17.svg", SortOrder = 6 },
        };
        foreach (var c in cats) c.Slug = c.Name.ToSlug();
        db.Categories.AddRange(cats);

        var occ = new[]
        {
            new Occasion { Name = "Sinh nhật", Description = "Chúc mừng tuổi mới rạng rỡ" },
            new Occasion { Name = "Tình yêu", Description = "Gửi trao yêu thương" },
            new Occasion { Name = "Chúc mừng", Description = "Tốt nghiệp, thăng chức, kỷ niệm" },
            new Occasion { Name = "Khai trương", Description = "Hồng phát, vạn sự như ý" },
            new Occasion { Name = "Cảm ơn - Tri ân", Description = "Lời cảm ơn chân thành" },
            new Occasion { Name = "Chia buồn", Description = "Thành kính phân ưu" },
        };
        foreach (var o in occ) o.Slug = o.Name.ToSlug();
        db.Occasions.AddRange(occ);

        // (tên, danh mục, dịp, giá, giá KM, mô tả ngắn, ý nghĩa, nổi bật)
        var data = new (string Name, int Cat, int Occ, decimal Price, decimal? Sale, string Short, string Meaning, bool Featured)[]
        {
            ("Bó hồng đỏ Say Đắm", 0, 1, 650000, 590000, "20 bông hồng đỏ Ecuador phối lá bạc, gói giấy Hàn cao cấp.", "Hoa hồng đỏ – biểu tượng của tình yêu nồng nàn và đam mê.", true),
            ("Bó hồng pastel Nắng Mai", 0, 0, 520000, null, "Hồng kem, hồng phấn và cúc họa mi nhẹ nhàng như nắng sớm.", "Sự dịu dàng, ngọt ngào và lời chúc an yên.", true),
            ("Bó tulip Hà Lan Mộng Mơ", 0, 1, 890000, 790000, "15 cành tulip nhập khẩu Hà Lan, tông hồng tím lãng mạn.", "Tulip tượng trưng cho tình yêu hoàn hảo.", true),
            ("Bó hướng dương Rạng Rỡ", 0, 2, 450000, 399000, "5 bông hướng dương tươi kết hợp baby trắng.", "Hướng dương – niềm tin, hy vọng và thành công.", false),
            ("Hộp hoa hồng Ngọt Ngào", 1, 0, 750000, null, "Hộp tròn nhung hồng cắm 25 bông hồng phấn và cẩm chướng.", "Món quà ngọt ngào cho người đặc biệt.", true),
            ("Hộp hoa Bình Minh", 1, 4, 680000, 620000, "Hồng cam, cẩm chướng và lá bạc trong hộp giấy kraft.", "Lời cảm ơn ấm áp như ánh bình minh.", false),
            ("Hộp hoa mẫu đơn Kiêu Sa", 1, 1, 1250000, 1150000, "Mẫu đơn nhập khẩu kết hợp hồng David Austin.", "Mẫu đơn – sự giàu sang, hạnh phúc viên mãn.", true),
            ("Hộp hoa Lavender Dịu Êm", 1, 4, 560000, null, "Oải hương khô và hồng tím trong hộp gỗ thông.", "Sự chung thủy và bình yên.", false),
            ("Giỏ hoa Thanh Xuân", 2, 2, 820000, 750000, "Giỏ mây tròn với hồng, cúc mẫu đơn và lá chanh.", "Chúc thanh xuân tươi đẹp, rực rỡ.", false),
            ("Giỏ hoa Hạnh Phúc", 2, 0, 950000, null, "Giỏ hoa đầy đặn tông hồng – trắng cho tiệc sinh nhật.", "Ngập tràn niềm vui và hạnh phúc.", true),
            ("Giỏ hoa Tri Ân Thầy Cô", 2, 4, 700000, 650000, "Giỏ hoa đồng tiền, hướng dương dành tặng thầy cô.", "Lòng biết ơn sâu sắc.", false),
            ("Kệ hoa Hồng Phát", 3, 3, 1800000, 1650000, "Kệ 2 tầng đồng tiền đỏ, hồng vàng và lan vũ nữ.", "Chúc khai trương hồng phát, buôn may bán đắt.", true),
            ("Kệ hoa Đại Cát", 3, 3, 2200000, null, "Kệ 3 tầng hoa hướng dương, hồng môn sang trọng.", "Đại cát đại lợi, vạn sự hanh thông.", false),
            ("Kệ hoa Thành Kính", 3, 5, 1500000, null, "Kệ hoa cúc trắng, lan trắng trang nghiêm.", "Thành kính phân ưu, tiễn biệt người đã khuất.", false),
            ("Hoa cưới Tinh Khôi", 4, 1, 990000, 890000, "Hoa cầm tay hồng trắng, cát tường và baby.", "Tình yêu thuần khiết, trăm năm hạnh phúc.", true),
            ("Hoa cưới Lãng Mạn", 4, 1, 1100000, null, "Hồng kem, mẫu đơn hồng dáng rủ thanh lịch.", "Lời hẹn ước dịu dàng trọn đời.", false),
            ("Lan hồ điệp Vàng Quý Phái", 5, 3, 2500000, 2300000, "Chậu 5 cành lan hồ điệp vàng trong chậu sứ.", "Sự giàu sang, phú quý và may mắn.", true),
            ("Lan hồ điệp Trắng Thanh Tao", 5, 4, 2100000, null, "Chậu 3 cành lan hồ điệp trắng tinh khôi.", "Sự thanh cao, tôn kính và tri ân.", false),
        };

        var products = new List<Product>();
        var rnd = new Random(7);
        for (var i = 0; i < data.Length; i++)
        {
            var d = data[i];
            var p = new Product
            {
                Name = d.Name,
                Slug = d.Name.ToSlug(),
                Sku = $"FL{(i + 1):000}",
                Category = cats[d.Cat],
                Occasion = occ[d.Occ],
                Price = d.Price,
                SalePrice = d.Sale,
                Image = $"/img/flowers/p{(i + 1):00}.svg",
                ShortDescription = d.Short,
                Meaning = d.Meaning,
                Description =
                    $"<p>{d.Short} Mỗi sản phẩm được florist của Floria tuyển chọn và cắm thủ công ngay trong ngày, đảm bảo hoa tươi khi đến tay người nhận.</p>" +
                    "<ul><li>Hoa tươi nhập mới mỗi sáng</li><li>Tặng kèm thiệp viết tay theo lời nhắn</li><li>Giao nhanh trong 2 giờ nội thành</li><li>Chụp ảnh sản phẩm trước khi giao</li></ul>" +
                    "<p><strong>Lưu ý:</strong> Do đặc tính hoa tươi theo mùa, màu sắc và loại hoa phụ có thể thay đổi nhẹ nhưng vẫn giữ nguyên phong cách và giá trị.</p>",
                Stock = rnd.Next(15, 60),
                Sold = rnd.Next(5, 180),
                IsFeatured = d.Featured,
                CreatedAt = DateTime.UtcNow.AddDays(-data.Length + i),
            };
            products.Add(p);
        }
        db.Products.AddRange(products);

        db.Sliders.AddRange(
            new Slider { Title = "Gửi yêu thương qua từng cánh hoa", Subtitle = "Hoa tươi cắm thủ công mỗi ngày – giao nhanh 2 giờ nội thành", Image = "/img/flowers/hero1.svg", Link = "/cua-hang", SortOrder = 1 },
            new Slider { Title = "Bộ sưu tập Tulip Hà Lan", Subtitle = "Giảm đến 15% cho đơn hàng đặt trước 3 ngày", Image = "/img/flowers/hero2.svg", Link = "/cua-hang?occasion=tinh-yeu", SortOrder = 2 },
            new Slider { Title = "Kệ hoa khai trương hồng phát", Subtitle = "Miễn phí giao hàng & lắp đặt tận nơi", Image = "/img/flowers/hero3.svg", Link = "/cua-hang?category=ke-hoa-khai-truong", SortOrder = 3 });

        var pcs = new[]
        {
            new PostCategory { Name = "Ý nghĩa các loài hoa" },
            new PostCategory { Name = "Mẹo chăm sóc hoa" },
            new PostCategory { Name = "Tin tức & Khuyến mãi" },
        };
        foreach (var c in pcs) c.Slug = c.Name.ToSlug();
        db.PostCategories.AddRange(pcs);

        var posts = new (string Title, int Cat, string Summary, string Img)[]
        {
            ("Ý nghĩa màu sắc hoa hồng bạn nên biết trước khi tặng", 0, "Mỗi màu hồng mang một thông điệp riêng: đỏ cho tình yêu, vàng cho tình bạn, trắng cho sự thuần khiết...", "/img/flowers/p01.svg"),
            ("7 cách giữ hoa tươi lâu gấp đôi tại nhà", 1, "Cắt chéo cành, thay nước mỗi ngày và tránh ánh nắng trực tiếp là những mẹo đơn giản giúp hoa bền hơn.", "/img/flowers/p02.svg"),
            ("Gợi ý chọn hoa sinh nhật theo cung hoàng đạo", 0, "Bạch Dương hợp hoa tulip đỏ, Kim Ngưu hợp hồng phấn... Cùng Floria khám phá nhé!", "/img/flowers/p10.svg"),
            ("Chăm sóc lan hồ điệp đúng cách cho hoa nở bền", 1, "Lan hồ điệp ưa ánh sáng tán xạ, tưới nước 1–2 lần mỗi tuần và tránh đọng nước ở gốc.", "/img/flowers/p17.svg"),
            ("Ưu đãi mùa cưới: Giảm 10% hoa cầm tay cô dâu", 2, "Từ nay đến hết tháng, mọi mẫu hoa cưới được giảm 10% và tặng kèm hoa cài áo chú rể.", "/img/flowers/p15.svg"),
            ("Floria ra mắt dịch vụ giao hoa trong 2 giờ", 2, "Đặt hoa trước 18h, nhận hoa trong 2 giờ tại khu vực nội thành – kèm ảnh chụp thực tế trước khi giao.", "/img/flowers/p04.svg"),
        };
        for (var i = 0; i < posts.Length; i++)
        {
            var x = posts[i];
            db.Posts.Add(new Post
            {
                Title = x.Title,
                Slug = x.Title.ToSlug(),
                Summary = x.Summary,
                Image = x.Img,
                PostCategory = pcs[x.Cat],
                Views = rnd.Next(30, 500),
                CreatedAt = DateTime.UtcNow.AddDays(-(posts.Length - i) * 4),
                Content =
                    $"<p>{x.Summary}</p>" +
                    "<h3>Vì sao điều này quan trọng?</h3><p>Hoa không chỉ là món quà – hoa là ngôn ngữ của cảm xúc. Hiểu đúng về hoa giúp món quà của bạn trở nên trọn vẹn và tinh tế hơn trong mắt người nhận.</p>" +
                    "<h3>Lời khuyên từ florist Floria</h3><ul><li>Chọn hoa theo sở thích và cá tính của người nhận.</li><li>Kết hợp thiệp viết tay để gửi gắm lời nhắn.</li><li>Đặt trước 1–2 ngày vào các dịp lễ lớn để có mẫu đẹp nhất.</li></ul>" +
                    "<p>Nếu bạn cần tư vấn thêm, hãy liên hệ hotline <strong>1900 6868</strong> – Floria luôn sẵn sàng hỗ trợ.</p>"
            });
        }

        var customer = new Customer { FullName = "Nguyễn Minh Anh", Email = "khach@floria.vn", Phone = "0901234567", Address = "123 Đường 3/2, Ninh Kiều, Cần Thơ", PasswordHash = Passwords.Hash("Khach@123") };
        db.Customers.Add(customer);
        await db.SaveChangesAsync();

        // Đơn hàng mẫu trải đều 6 tháng gần nhất để biểu đồ doanh thu có dữ liệu
        var names = new[] { "Trần Thu Hà", "Lê Hoàng Nam", "Phạm Ngọc Mai", "Võ Thanh Tùng", "Đỗ Khánh Linh", "Huỳnh Gia Bảo" };
        for (var i = 0; i < 26; i++)
        {
            var created = DateTime.UtcNow.AddDays(-rnd.Next(0, 170)).AddHours(-rnd.Next(0, 20));
            var picks = products.OrderBy(_ => rnd.Next()).Take(rnd.Next(1, 3)).ToList();
            var order = new Order
            {
                Code = "FL" + created.ToString("yyMMdd") + rnd.Next(1000, 9999),
                CustomerId = i % 4 == 0 ? customer.Id : null,
                SenderName = i % 4 == 0 ? customer.FullName : names[i % names.Length],
                SenderPhone = "09" + rnd.Next(10000000, 99999999),
                ReceiverName = names[(i + 2) % names.Length],
                ReceiverPhone = "08" + rnd.Next(10000000, 99999999),
                Address = $"{rnd.Next(1, 300)} Nguyễn Văn Cừ, Ninh Kiều, Cần Thơ",
                DeliveryDate = created.AddDays(1).Date,
                DeliverySlot = Helpers.DeliverySlots[rnd.Next(Helpers.DeliverySlots.Length)],
                CardMessage = "Chúc bạn luôn vui vẻ và hạnh phúc!",
                PaymentMethod = i % 3 == 0 ? PaymentMethod.COD : PaymentMethod.VNPay,
                CreatedAt = created,
                UpdatedAt = created,
            };
            foreach (var p in picks)
                order.Details.Add(new OrderDetail { ProductId = p.Id, ProductName = p.Name, Image = p.Image, Price = p.FinalPrice, Quantity = rnd.Next(1, 3) });
            order.Subtotal = order.Details.Sum(d => d.Price * d.Quantity);
            order.ShippingFee = order.Subtotal >= 500000 ? 0 : 30000;
            order.Total = order.Subtotal + order.ShippingFee;
            var age = (DateTime.UtcNow - created).TotalDays;
            order.Status = age > 7 ? (i % 9 == 0 ? OrderStatus.DaHuy : OrderStatus.HoanThanh)
                : (OrderStatus)(i % 3);
            order.PaymentStatus = order.PaymentMethod == PaymentMethod.VNPay || order.Status == OrderStatus.HoanThanh
                ? PaymentStatus.DaThanhToan : PaymentStatus.ChuaThanhToan;
            if (order.PaymentMethod == PaymentMethod.VNPay)
                order.Transactions.Add(new PaymentTransaction
                {
                    Provider = "VNPAY", TxnRef = order.Code, TransactionNo = rnd.Next(10000000, 99999999).ToString(),
                    Amount = order.Total, BankCode = "NCB", ResponseCode = "00", Message = "Giao dịch thành công", IsSuccess = true, CreatedAt = created
                });
            db.Orders.Add(order);
        }

        db.ContactMessages.Add(new ContactMessage { FullName = "Lê Thị Hồng", Email = "hong@example.com", Phone = "0912345678", Message = "Shop có nhận cắm hoa sự kiện hội nghị 200 khách không ạ?" });
        await db.SaveChangesAsync();
    }
}
