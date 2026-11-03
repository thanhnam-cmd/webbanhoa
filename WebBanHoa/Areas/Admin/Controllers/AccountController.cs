using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanHoa.Data;
using WebBanHoa.Models;

namespace WebBanHoa.Areas.Admin.Controllers;

public class AccountController : AdminBaseController
{
    public AccountController(AppDbContext db) : base(db) { }

    [AllowAnonymous, HttpGet("admin/dang-nhap")]
    public IActionResult Login(string? returnUrl) => View(new AdminLoginViewModel { ReturnUrl = returnUrl });

    [AllowAnonymous, HttpPost("admin/dang-nhap"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(AdminLoginViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);
        var u = await Db.AdminUsers.FirstOrDefaultAsync(x => x.Username == vm.Username.Trim());
        if (u == null || !Passwords.Verify(u.PasswordHash, vm.Password))
        {
            ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng");
            return View(vm);
        }
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, u.Id.ToString()), new(ClaimTypes.Name, u.FullName), new("username", u.Username) };
        await HttpContext.SignInAsync(AuthSchemes.Admin, new ClaimsPrincipal(new ClaimsIdentity(claims, AuthSchemes.Admin)));
        return Url.IsLocalUrl(vm.ReturnUrl) ? Redirect(vm.ReturnUrl!) : RedirectToAction("Index", "Dashboard");
    }

    [HttpPost("admin/dang-xuat"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(AuthSchemes.Admin);
        return RedirectToAction(nameof(Login));
    }

    [HttpGet("admin/doi-mat-khau")]
    public IActionResult ChangePassword() => View();

    [HttpPost("admin/doi-mat-khau"), ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
    {
        var id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var u = await Db.AdminUsers.FindAsync(id);
        if (u == null || !Passwords.Verify(u.PasswordHash, currentPassword ?? "")) ModelState.AddModelError("", "Mật khẩu hiện tại không đúng");
        else if (string.IsNullOrEmpty(newPassword) || newPassword.Length < 6) ModelState.AddModelError("", "Mật khẩu mới tối thiểu 6 ký tự");
        else if (newPassword != confirmPassword) ModelState.AddModelError("", "Mật khẩu xác nhận không khớp");
        if (!ModelState.IsValid) return View();
        u!.PasswordHash = Passwords.Hash(newPassword);
        await Db.SaveChangesAsync();
        Toast("Đổi mật khẩu thành công");
        return RedirectToAction("Index", "Dashboard");
    }
}
