using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using WebBanHoa;
using WebBanHoa.Data;
using WebBanHoa.Services;

// Cho phép lưu DateTime không kèm múi giờ khi dùng PostgreSQL (môi trường Render)
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// Render cấp cổng qua biến môi trường PORT
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port)) builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// ---------- Cơ sở dữ liệu ----------
// Mặc định dùng SQL Server (chạy local). Khi có DATABASE_URL (Render PostgreSQL) sẽ tự chuyển sang PostgreSQL.
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
builder.Services.AddDbContext<AppDbContext>(opt =>
{
    if (!string.IsNullOrEmpty(databaseUrl))
        opt.UseNpgsql(DbConfig.PostgresFromUrl(databaseUrl));
    else if (Environment.GetEnvironmentVariable("DB_PROVIDER") == "sqlite")
    {
        // Bản demo trên Render: SQLite nhúng, dữ liệu mẫu tự tạo lại khi khởi động
        Directory.CreateDirectory("data");
        opt.UseSqlite("Data Source=data/webbanhoa.db");
    }
    else
        opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddDataProtection().PersistKeysToDbContext<AppDbContext>().SetApplicationName("WebBanHoa");

// ---------- Xác thực: 2 cookie riêng cho Khách hàng và Quản trị ----------
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, o =>
    {
        o.Cookie.Name = "floria.customer";
        o.LoginPath = "/tai-khoan/dang-nhap";
        o.AccessDeniedPath = "/tai-khoan/dang-nhap";
        o.ExpireTimeSpan = TimeSpan.FromDays(14);
        o.SlidingExpiration = true;
    })
    .AddCookie(AuthSchemes.Admin, o =>
    {
        o.Cookie.Name = "floria.admin";
        o.LoginPath = "/admin/dang-nhap";
        o.AccessDeniedPath = "/admin/dang-nhap";
        o.ExpireTimeSpan = TimeSpan.FromHours(8);
    });

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(o =>
{
    o.Cookie.Name = "floria.session";
    o.IdleTimeout = TimeSpan.FromDays(2);
    o.Cookie.HttpOnly = true;
    o.Cookie.IsEssential = true;
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<CartService>();
builder.Services.AddScoped<MediaService>();
builder.Services.AddSingleton(builder.Configuration.GetSection("VnPay").Get<VnPayOptions>() ?? new VnPayOptions());
builder.Services.AddSingleton<VnPayService>();
builder.Services.AddControllersWithViews(o => o.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true);

builder.Services.Configure<ForwardedHeadersOptions>(o =>
{
    o.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    o.KnownNetworks.Clear();
    o.KnownProxies.Clear();
});

var app = builder.Build();

// Tạo CSDL + dữ liệu mẫu ở lần chạy đầu tiên
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    // dotnet run -- --export-sql Database/WebBanHoa.sql : xuất script tạo bảng (dùng cho báo cáo / tạo CSDL thủ công)
    var exportIdx = Array.IndexOf(args, "--export-sql");
    if (exportIdx >= 0)
    {
        await File.WriteAllTextAsync(args.ElementAtOrDefault(exportIdx + 1) ?? "WebBanHoa.sql", db.Database.GenerateCreateScript());
        return;
    }
    await DbSeeder.SeedAsync(db);
}

app.UseForwardedHeaders();
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStatusCodePagesWithReExecute("/Home/NotFound404");

app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute("areas", "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");

app.Run();
