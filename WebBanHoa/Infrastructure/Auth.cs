using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Npgsql;

namespace WebBanHoa;

public static class AuthSchemes
{
    public const string Admin = "AdminCookie";
}

public static class Passwords
{
    private static readonly PasswordHasher<object> Hasher = new();
    public static string Hash(string password) => Hasher.HashPassword(null!, password);
    public static bool Verify(string hash, string password) =>
        !string.IsNullOrEmpty(hash) && Hasher.VerifyHashedPassword(null!, hash, password) != PasswordVerificationResult.Failed;
}

public static class UserExtensions
{
    public static int? CustomerId(this ClaimsPrincipal user) =>
        int.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;
}

public static class DbConfig
{
    /// <summary>Chuyển chuỗi postgres://user:pass@host:port/db (Render) sang dạng Npgsql.</summary>
    public static string PostgresFromUrl(string url)
    {
        if (!url.StartsWith("postgres")) return url;
        var uri = new Uri(url);
        var userInfo = uri.UserInfo.Split(':', 2);
        var csb = new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.Port > 0 ? uri.Port : 5432,
            Username = Uri.UnescapeDataString(userInfo[0]),
            Password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : "",
            Database = uri.AbsolutePath.TrimStart('/'),
            SslMode = uri.Host.Contains('.') ? SslMode.Require : SslMode.Prefer,
        };
        return csb.ConnectionString;
    }
}
