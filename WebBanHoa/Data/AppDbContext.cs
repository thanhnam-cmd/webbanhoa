using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebBanHoa.Models;

namespace WebBanHoa.Data;

public class AppDbContext : DbContext, IDataProtectionKeyContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Occasion> Occasions => Set<Occasion>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Slider> Sliders => Set<Slider>();
    public DbSet<PostCategory> PostCategories => Set<PostCategory>();
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<AdminUser> AdminUsers => Set<AdminUser>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderDetail> OrderDetails => Set<OrderDetail>();
    public DbSet<PaymentTransaction> PaymentTransactions => Set<PaymentTransaction>();
    public DbSet<ContactMessage> ContactMessages => Set<ContactMessage>();
    public DbSet<MediaFile> MediaFiles => Set<MediaFile>();
    public DbSet<DataProtectionKey> DataProtectionKeys => Set<DataProtectionKey>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Category>().HasIndex(x => x.Slug).IsUnique();
        b.Entity<Occasion>().HasIndex(x => x.Slug).IsUnique();
        b.Entity<Product>().HasIndex(x => x.Slug).IsUnique();
        b.Entity<PostCategory>().HasIndex(x => x.Slug).IsUnique();
        b.Entity<Post>().HasIndex(x => x.Slug).IsUnique();
        b.Entity<Customer>().HasIndex(x => x.Email).IsUnique();
        b.Entity<AdminUser>().HasIndex(x => x.Username).IsUnique();
        b.Entity<Order>().HasIndex(x => x.Code).IsUnique();

        b.Entity<Product>().HasOne(p => p.Category).WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Product>().HasOne(p => p.Occasion).WithMany(o => o.Products)
            .HasForeignKey(p => p.OccasionId).OnDelete(DeleteBehavior.SetNull);
        b.Entity<Post>().HasOne(p => p.PostCategory).WithMany(c => c.Posts)
            .HasForeignKey(p => p.PostCategoryId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Order>().HasOne(o => o.Customer).WithMany(c => c.Orders)
            .HasForeignKey(o => o.CustomerId).OnDelete(DeleteBehavior.SetNull);
        b.Entity<OrderDetail>().HasOne(d => d.Order).WithMany(o => o.Details)
            .HasForeignKey(d => d.OrderId).OnDelete(DeleteBehavior.Cascade);
        b.Entity<OrderDetail>().HasOne(d => d.Product).WithMany()
            .HasForeignKey(d => d.ProductId).OnDelete(DeleteBehavior.SetNull);
        b.Entity<PaymentTransaction>().HasOne(t => t.Order).WithMany(o => o.Transactions)
            .HasForeignKey(t => t.OrderId).OnDelete(DeleteBehavior.Cascade);

        // SQLite không sắp xếp/tính tổng được kiểu decimal → lưu dạng số thực
        if (Database.IsSqlite())
            foreach (var p in b.Model.GetEntityTypes().SelectMany(t => t.GetProperties())
                         .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
                p.SetValueConverter(new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<decimal, double>(v => (double)v, v => (decimal)v));
    }
}
