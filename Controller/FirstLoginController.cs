using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KintaiSystem.Data;
using KintaiSystem.Models.Entities;
using YourProjectName.Models.ViewModels;

namespace KintaiSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FirstLoginController : ControllerBase 
    {
        private readonly AppDbContext _context;
        private readonly IPasswordHasher<Employee> _passwordHasher;

        // 設計: エラーコードに対応する文言定義
        private static readonly Dictionary<string, string> ErrorMessages = new()
        {
            { "E-0001", "入力内容をご確認のうえ、再度入力してください。" },
            { "E-0002", "指定された桁数で入力してください。入力内容をご確認のうえ、再度入力してください。" },
            { "E-0003", "指定された文字種（英数字・記号等）で入力してください。入力内容をご確認のうえ、再度入力してください。" },
            { "E-0004", "パスワードは指定された形式（英大文字・小文字・数字を含む形式等）で入力してください。入力内容をご確認のうえ、再度入力してください。" },
            { "E-0005", "メールアドレスまたはパスワードに誤りがあります。正しく入力されているかご確認のうえ、再度お試しください。" },
            { "E-0006", "パスワードと確認用パスワードが一致していません。両方に同じパスワードを入力してください。" },
        };

        // 依存性注入
        public FirstLoginController(
            AppDbContext context,
            IPasswordHasher<Employee> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        // 初回パスワード登録およびログイン処理 (POST: api/firstlogin)
        [HttpPost]
        public async Task<IActionResult> FirstLogin([FromBody] FirstLoginViewModel model)
        {
            // 入力チェック
            if (!ModelState.IsValid)
            {
                return BadRequest(new { isSuccess = false, errorCode = "E-0001" });
            }

            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Email == model.Email);

            // 該当ユーザー不在、または初回ログイン済み (IsFirstLogin == false) の場合は E-0005
            if (employee == null || !employee.IsFirstLogin)
            {
                return BadRequest(new
                {
                    isSuccess = false,
                    errorCode = "E-0005",
                    message = ErrorMessages["E-0005"]
                });
            }

            // 初回パスワードのハッシュ化・保存・フラグ解除
            employee.PasswordHash = _passwordHasher.HashPassword(employee, model.NewPassword);
            employee.IsFirstLogin = false;
            await _context.SaveChangesAsync();

            // サインイン処理 (認証クッキー発行)
            await SignInEmployeeAsync(employee);

            return Ok(new
            {
                isSuccess = true,
                redirectUrl = "/timestamp.html"
            });
        }

        // サインイン処理 (クレーム生成とクッキー発行)
        private async Task SignInEmployeeAsync(Employee employee)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, employee.Id.ToString()),
                new Claim(ClaimTypes.Name, employee.Name),
                new Claim("EmployeeNumber", employee.EmployeeNumber),
                new Claim(ClaimTypes.Role, employee.IsAdmin ? "Admin" : "User")
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity)
            );
        }
    }
}