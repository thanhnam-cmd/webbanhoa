"""Sinh sơ đồ UML (SVG) cho báo cáo: usecase, ERD, kiến trúc, tuần tự thanh toán.
Chạy: python tools/gen_diagrams.py  -> docs/diagrams/*.svg
"""
import os
from html import escape

OUT = os.path.join(os.path.dirname(__file__), "..", "docs", "diagrams")
FONT = "font-family='Segoe UI, Arial, sans-serif'"
INK, ROSE, LINE, FILL = "#2A2220", "#C8506A", "#8C7F79", "#FFF6F5"


def svg(w, h, body):
    return (f"<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 {w} {h}' width='{w}' height='{h}' {FONT}>"
            f"<defs><marker id='arr' viewBox='0 0 10 10' refX='9' refY='5' markerWidth='8' markerHeight='8' orient='auto'>"
            f"<path d='M0 0L10 5L0 10z' fill='{INK}'/></marker>"
            f"<marker id='open' viewBox='0 0 10 10' refX='9' refY='5' markerWidth='9' markerHeight='9' orient='auto'>"
            f"<path d='M0 0L10 5L0 10' fill='none' stroke='{INK}'/></marker></defs>"
            f"<rect width='{w}' height='{h}' fill='#fff'/>{body}</svg>")


def actor(x, y, name):
    return (f"<g stroke='{INK}' stroke-width='2' fill='none'><circle cx='{x}' cy='{y}' r='14'/>"
            f"<path d='M{x} {y+14}v40M{x-24} {y+30}h48M{x} {y+54}l-20 34M{x} {y+54}l20 34'/></g>"
            f"<text x='{x}' y='{y+112}' text-anchor='middle' font-size='15' font-weight='600' fill='{INK}'>{escape(name)}</text>")


def oval(cx, cy, text, w=210, h=46):
    return (f"<ellipse cx='{cx}' cy='{cy}' rx='{w/2}' ry='{h/2}' fill='{FILL}' stroke='{ROSE}' stroke-width='1.6'/>"
            f"<text x='{cx}' y='{cy+5}' text-anchor='middle' font-size='13.5' fill='{INK}'>{escape(text)}</text>")


def line(x1, y1, x2, y2, dash=False, arrow=None):
    d = " stroke-dasharray='6 4'" if dash else ""
    m = f" marker-end='url(#{arrow})'" if arrow else ""
    return f"<line x1='{x1}' y1='{y1}' x2='{x2}' y2='{y2}' stroke='{LINE}' stroke-width='1.4'{d}{m}/>"


def usecase(title, left_actor, left_cases, right_actor=None, right_cases=None, include=None):
    """include: list of (case_text, target_text) cho quan hệ <<include>>"""
    rows = max(len(left_cases), len(right_cases or []))
    h = 120 + rows * 62
    w = 1000 if right_actor else 820
    b = [f"<rect x='150' y='50' width='{w-300 if right_actor else w-230}' height='{h-80}' rx='14' fill='none' stroke='{INK}' stroke-width='1.5'/>",
         f"<text x='{(w)/2}' y='38' text-anchor='middle' font-size='17' font-weight='700' fill='{INK}'>{escape(title)}</text>"]
    ay = 50 + (h - 80) / 2 - 60
    b.append(actor(80, ay, left_actor))
    lx = 360 if right_actor else 420
    for i, c in enumerate(left_cases):
        cy = 95 + i * 62
        b.append(line(105, ay + 40, lx - 105, cy))
        b.append(oval(lx, cy, c))
    if right_actor:
        b.append(actor(w - 80, ay, right_actor))
        rx = w - 360
        for i, c in enumerate(right_cases):
            cy = 95 + i * 62
            b.append(line(w - 105, ay + 40, rx + 105, cy))
            b.append(oval(rx, cy, c))
    if include:
        for src, tgt in include:
            pass
    return svg(w, h, "".join(b))


def table_box(x, y, name, fields, w=230):
    h = 34 + len(fields) * 21 + 8
    s = [f"<rect x='{x}' y='{y}' width='{w}' height='{h}' rx='8' fill='#fff' stroke='{INK}' stroke-width='1.4'/>",
         f"<path d='M{x} {y+8}a8 8 0 0 1 8-8h{w-16}a8 8 0 0 1 8 8v26h-{w}z' fill='{ROSE}'/>",
         f"<text x='{x + w/2}' y='{y+22}' text-anchor='middle' font-size='14' font-weight='700' fill='#fff'>{escape(name)}</text>"]
    for i, (f, t) in enumerate(fields):
        yy = y + 52 + i * 21
        pk = f.startswith("PK ")
        fk = f.startswith("FK ")
        label = f[3:] if pk or fk else f
        tag = "🔑 " if pk else ("↗ " if fk else "")
        s.append(f"<text x='{x+10}' y='{yy}' font-size='12.5' fill='{INK}' font-weight='{700 if pk else 400}'>{tag}{escape(label)}</text>")
        s.append(f"<text x='{x+w-10}' y='{yy}' text-anchor='end' font-size='11.5' fill='{LINE}'>{escape(t)}</text>")
    return "".join(s), h


def erd():
    T = {
        "Categories": (40, 60, [("PK Id", "int"), ("Name", "nvarchar(100)"), ("Slug", "nvarchar(120)"), ("Description", "nvarchar(300)"), ("Image", "nvarchar(300)"), ("SortOrder", "int"), ("IsActive", "bit")]),
        "Occasions": (40, 300, [("PK Id", "int"), ("Name", "nvarchar(100)"), ("Slug", "nvarchar(120)"), ("Description", "nvarchar(300)"), ("IsActive", "bit")]),
        "Products": (330, 60, [("PK Id", "int"), ("Name", "nvarchar(150)"), ("Slug", "nvarchar(170)"), ("Sku", "nvarchar(30)"), ("FK CategoryId", "int"), ("FK OccasionId", "int?"), ("Price", "decimal(18,0)"), ("SalePrice", "decimal(18,0)?"), ("Image", "nvarchar(300)"), ("ShortDescription", "nvarchar(400)"), ("Description", "nvarchar(max)"), ("Meaning", "nvarchar(400)"), ("Stock / Sold", "int"), ("IsFeatured / IsActive", "bit"), ("CreatedAt / UpdatedAt", "datetime2")]),
        "OrderDetails": (630, 60, [("PK Id", "int"), ("FK OrderId", "int"), ("FK ProductId", "int?"), ("ProductName", "nvarchar(150)"), ("Image", "nvarchar(300)"), ("Price", "decimal(18,0)"), ("Quantity", "int")]),
        "Orders": (930, 60, [("PK Id", "int"), ("Code", "nvarchar(20)"), ("FK CustomerId", "int?"), ("SenderName/Phone/Email", "nvarchar"), ("ReceiverName/Phone", "nvarchar"), ("Address", "nvarchar(300)"), ("DeliveryDate", "datetime2"), ("DeliverySlot", "nvarchar(30)"), ("CardMessage / Note", "nvarchar"), ("Subtotal/ShippingFee/Total", "decimal"), ("PaymentMethod", "int"), ("PaymentStatus", "int"), ("Status", "int"), ("CancelReason", "nvarchar(300)"), ("CreatedAt / UpdatedAt", "datetime2")]),
        "Customers": (930, 470, [("PK Id", "int"), ("FullName", "nvarchar(100)"), ("Email", "nvarchar(150)"), ("Phone", "nvarchar(20)"), ("Address", "nvarchar(300)"), ("PasswordHash", "nvarchar(200)"), ("IsLocked", "bit"), ("CreatedAt", "datetime2")]),
        "PaymentTransactions": (630, 300, [("PK Id", "int"), ("FK OrderId", "int"), ("Provider", "nvarchar(30)"), ("TxnRef", "nvarchar(50)"), ("TransactionNo", "nvarchar(50)"), ("Amount", "decimal(18,0)"), ("BankCode", "nvarchar(20)"), ("ResponseCode", "nvarchar(10)"), ("Message", "nvarchar(300)"), ("IsSuccess", "bit"), ("CreatedAt", "datetime2")]),
        "PostCategories": (40, 500, [("PK Id", "int"), ("Name", "nvarchar(100)"), ("Slug", "nvarchar(120)"), ("IsActive", "bit")]),
        "Posts": (330, 470, [("PK Id", "int"), ("Title", "nvarchar(200)"), ("Slug", "nvarchar(220)"), ("Summary", "nvarchar(500)"), ("Content", "nvarchar(max)"), ("Image", "nvarchar(300)"), ("FK PostCategoryId", "int"), ("Views", "int"), ("IsActive", "bit"), ("CreatedAt", "datetime2")]),
        "Sliders": (630, 580, [("PK Id", "int"), ("Title", "nvarchar(150)"), ("Subtitle", "nvarchar(300)"), ("Image / Link", "nvarchar(300)"), ("SortOrder", "int"), ("IsActive", "bit")]),
        "AdminUsers": (40, 660, [("PK Id", "int"), ("Username", "nvarchar(50)"), ("FullName", "nvarchar(100)"), ("PasswordHash", "nvarchar(200)"), ("CreatedAt", "datetime2")]),
        "ContactMessages": (930, 700, [("PK Id", "int"), ("FullName", "nvarchar(100)"), ("Email / Phone", "nvarchar"), ("Message", "nvarchar(2000)"), ("IsRead", "bit"), ("CreatedAt", "datetime2")]),
    }
    body, boxes = [], {}
    for name, (x, y, f) in T.items():
        s, h = table_box(x, y, name, f, w=250)
        body.append(s)
        boxes[name] = (x, y, 250, h)

    def rel(a, b, side_a="r", side_b="l", ya=None, yb=None, label="1 - n"):
        xa, ya0, wa, ha = boxes[a]
        xb, yb0, wb, hb = boxes[b]
        p1 = (xa + wa if side_a == "r" else xa, ya0 + (ya or ha / 2))
        p2 = (xb if side_b == "l" else (xb + wb if side_b == "r" else xb + wb / 2), yb0 + (yb or hb / 2) if side_b in "lr" else yb0)
        mx = (p1[0] + p2[0]) / 2
        body.insert(0, f"<path d='M{p1[0]} {p1[1]} C{mx} {p1[1]} {mx} {p2[1]} {p2[0]} {p2[1]}' fill='none' stroke='{LINE}' stroke-width='1.6' marker-end='url(#arr)'/>")
        body.append(f"<text x='{mx}' y='{(p1[1]+p2[1])/2 - 6}' text-anchor='middle' font-size='11.5' fill='{ROSE}' font-weight='700'>{label}</text>")

    rel("Categories", "Products", yb=125)
    rel("Occasions", "Products", yb=146)
    rel("Products", "OrderDetails", yb=95)
    rel("Orders", "OrderDetails", side_a="l", side_b="r", ya=40, yb=75)
    rel("Orders", "PaymentTransactions", side_a="l", side_b="r", ya=200, yb=60)
    rel("Customers", "Orders", side_a="t", side_b="t", label="1 - n") if False else None
    body.insert(0, f"<path d='M1055 470 L1055 {boxes['Orders'][1]+boxes['Orders'][3]}' stroke='{LINE}' stroke-width='1.6' marker-end='url(#arr)'/>")
    body.append(f"<text x='1065' y='{(470+boxes['Orders'][1]+boxes['Orders'][3])/2}' font-size='11.5' fill='{ROSE}' font-weight='700'>1 - n</text>")
    rel("PostCategories", "Posts", yb=150)
    body.append(f"<text x='640' y='30' text-anchor='middle' font-size='18' font-weight='700' fill='{INK}'>Sơ đồ cơ sở dữ liệu – Website bán hoa Floria</text>")
    return svg(1220, 930, "".join(body))


def architecture():
    b = [f"<text x='500' y='34' text-anchor='middle' font-size='18' font-weight='700' fill='{INK}'>Kiến trúc hệ thống (ASP.NET Core MVC)</text>"]

    def box(x, y, w, h, t, sub, c=FILL, s=ROSE):
        return (f"<rect x='{x}' y='{y}' width='{w}' height='{h}' rx='12' fill='{c}' stroke='{s}' stroke-width='1.6'/>"
                f"<text x='{x+w/2}' y='{y+28}' text-anchor='middle' font-size='15' font-weight='700' fill='{INK}'>{escape(t)}</text>"
                + "".join(f"<text x='{x+w/2}' y='{y+52+i*19}' text-anchor='middle' font-size='12.5' fill='#5C4F4A'>{escape(l)}</text>" for i, l in enumerate(sub)))
    b.append(box(30, 70, 200, 130, "Trình duyệt", ["Khách hàng / Quản trị", "HTML5 · CSS3 · JS", "Responsive"], "#F3F6FB", "#5B8DEF"))
    b.append(box(300, 60, 400, 300, "", [], "#fff", INK))
    b.append(f"<text x='500' y='84' text-anchor='middle' font-size='15' font-weight='700' fill='{INK}'>Web Server – ASP.NET Core (.NET 9)</text>")
    b.append(box(320, 100, 170, 110, "Controller", ["Home, Shop, Cart,", "Checkout, Payment,", "Account, Admin/*"]))
    b.append(box(510, 100, 170, 110, "View (Razor)", ["_Layout, cshtml", "Tag Helpers", "Partial views"]))
    b.append(box(320, 230, 170, 110, "Model", ["Entities", "ViewModels", "Validation"]))
    b.append(box(510, 230, 170, 110, "Services", ["CartService", "VnPayService", "MediaService"]))
    b.append(box(780, 70, 200, 120, "SQL Server", ["EF Core 9", "(PostgreSQL khi", "deploy Render)"], "#E7EEE3", "#5E7D58"))
    b.append(box(780, 230, 200, 120, "Cổng VNPay", ["HMAC-SHA512", "ReturnUrl + IPN", "(Sandbox)"], "#E8F1FF", "#005BAA"))
    b.append(line(230, 135, 300, 135, arrow="arr") + line(300, 165, 230, 165, arrow="arr"))
    b.append(f"<text x='265' y='125' text-anchor='middle' font-size='11' fill='{LINE}'>HTTP</text>")
    b.append(line(700, 130, 780, 130, arrow="arr") + line(700, 290, 780, 290, arrow="arr") + line(780, 310, 700, 310, dash=True, arrow="arr"))
    return svg(1010, 390, "".join(b))


def sequence():
    parts = ["Khách hàng", "Website Floria", "CSDL", "Cổng VNPay"]
    xs = [110, 380, 620, 860]
    steps = [
        (0, 1, "1. Đặt hàng, chọn thanh toán VNPay", False),
        (1, 2, "2. Lưu đơn (Chờ xác nhận, Chưa thanh toán)", False),
        (1, 1, "3. Tạo URL thanh toán + ký HMAC-SHA512", False),
        (1, 0, "4. Chuyển hướng tới cổng VNPay", True),
        (0, 3, "5. Chọn ngân hàng, nhập thẻ, OTP", False),
        (3, 1, "6. Redirect ReturnUrl kèm vnp_SecureHash", False),
        (1, 1, "7. Kiểm tra chữ ký, số tiền, mã phản hồi", False),
        (1, 2, "8. Lưu giao dịch, cập nhật Đã thanh toán", False),
        (3, 1, "9. IPN xác nhận (server-to-server)", True),
        (1, 0, "10. Hiển thị kết quả thanh toán", True),
    ]
    h = 120 + len(steps) * 48 + 30
    b = [f"<text x='490' y='30' text-anchor='middle' font-size='18' font-weight='700' fill='{INK}'>Sơ đồ tuần tự thanh toán online VNPay</text>"]
    for p, x in zip(parts, xs):
        b.append(f"<rect x='{x-80}' y='50' width='160' height='40' rx='8' fill='{FILL}' stroke='{ROSE}' stroke-width='1.6'/>"
                 f"<text x='{x}' y='75' text-anchor='middle' font-size='14' font-weight='700' fill='{INK}'>{p}</text>"
                 f"<line x1='{x}' y1='90' x2='{x}' y2='{h-20}' stroke='{LINE}' stroke-dasharray='5 5'/>")
    for i, (a, c, t, dash) in enumerate(steps):
        y = 125 + i * 48
        if a == c:
            x = xs[a]
            b.append(f"<path d='M{x} {y-8} h50 v18 h-46' fill='none' stroke='{INK}' stroke-width='1.4' marker-end='url(#arr)'/>")
            b.append(f"<text x='{x+58}' y='{y+4}' font-size='12.5' fill='{INK}'>{escape(t)}</text>")
        else:
            b.append(f"<line x1='{xs[a]}' y1='{y}' x2='{xs[c]}' y2='{y}' stroke='{INK}' stroke-width='1.4'{' stroke-dasharray=\"6 4\"' if dash else ''} marker-end='url(#arr)'/>")
            b.append(f"<text x='{(xs[a]+xs[c])/2}' y='{y-7}' text-anchor='middle' font-size='12.5' fill='{INK}'>{escape(t)}</text>")
    return svg(980, h, "".join(b))


def main():
    os.makedirs(OUT, exist_ok=True)
    files = {
        "uc-tong-quat": usecase("Sơ đồ Use Case tổng quát", "Khách hàng",
                                ["Xem & tìm kiếm sản phẩm", "Lọc, sắp xếp sản phẩm", "Quản lý giỏ hàng", "Đặt hàng & thanh toán", "Đăng ký / Đăng nhập", "Theo dõi, tra cứu đơn hàng", "Xem tin tức, gửi liên hệ"],
                                "Quản trị viên",
                                ["Đăng nhập quản trị", "Xem thống kê doanh thu", "Quản lý danh mục, dịp tặng", "Quản lý sản phẩm", "Quản lý slide, tin tức", "Quản lý đơn hàng", "Quản lý khách hàng, liên hệ"]),
        "uc-khach-hang": usecase("Use Case chức năng khách hàng", "Khách hàng",
                                 ["Đăng ký tài khoản", "Đăng nhập / Đăng xuất", "Xem sản phẩm theo danh mục", "Xem sản phẩm theo dịp tặng", "Tìm kiếm, lọc giá, sắp xếp", "Xem chi tiết sản phẩm", "Thêm / sửa / xóa giỏ hàng", "Đặt hàng, viết thiệp", "Thanh toán VNPay / COD", "Xem lịch sử, hủy đơn hàng", "Tra cứu đơn hàng"]),
        "uc-danh-muc": usecase("Use Case quản lý danh mục sản phẩm", "Quản trị viên", ["Xem danh sách danh mục", "Tìm kiếm danh mục", "Thêm danh mục", "Sửa danh mục", "Xóa danh mục"]),
        "uc-dip-tang": usecase("Use Case quản lý dịp tặng", "Quản trị viên", ["Xem danh sách dịp tặng", "Tìm kiếm dịp tặng", "Thêm dịp tặng", "Sửa dịp tặng", "Xóa dịp tặng"]),
        "uc-san-pham": usecase("Use Case quản lý sản phẩm", "Quản trị viên", ["Xem danh sách sản phẩm", "Tìm kiếm, lọc theo danh mục", "Thêm sản phẩm (tải ảnh)", "Sửa sản phẩm", "Ẩn / hiện sản phẩm", "Xóa sản phẩm"]),
        "uc-slide": usecase("Use Case quản lý slide", "Quản trị viên", ["Xem danh sách slide", "Thêm slide", "Sửa slide", "Xóa slide"]),
        "uc-don-hang": usecase("Use Case quản lý đơn hàng", "Quản trị viên", ["Xem, lọc danh sách đơn hàng", "Xem chi tiết đơn hàng", "Duyệt đơn / cập nhật trạng thái", "Xác nhận thanh toán", "Hủy đơn hàng (hoàn kho)", "In hóa đơn"]),
        "uc-tin-tuc": usecase("Use Case quản lý danh mục tin & tin tức", "Quản trị viên", ["Quản lý danh mục tin", "Xem danh sách tin tức", "Thêm bài viết", "Sửa bài viết", "Xóa bài viết"]),
        "erd": erd(),
        "kien-truc": architecture(),
        "tuan-tu-vnpay": sequence(),
    }
    for n, s in files.items():
        with open(os.path.join(OUT, n + ".svg"), "w", encoding="utf8") as fp:
            fp.write(s)
    print("OK", len(files))


if __name__ == "__main__":
    main()
