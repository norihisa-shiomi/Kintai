using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using KintaiSystem.Models.Entities;
using KintaiSystem.Services;

namespace KintaiSystem.Controllers
{
    public class LoginController : Controller
    {
        private readonly LoginService _loginService;

        private static readonly Dictionary<string, string> ErrorMessages = new()
        {
            { "E-0001", "入力内容をご確認のうえ、再度入力してください。" },
            { "E-0002", "指定された桁数で入力してください。入力内容をご確認のうえ、再度入力してください。" },
            { "E-0003", "指定された文字種（英数字・記号等）で入力してください。入力内容をご確認のうえ、再度入力してください。" },
            { "E-0004", "パスワードは指定された形式（英大文字・小文字・数字を含む形式等）で入力してください。入力内容をご確認のうえ、再度入力してください。" },
            { "E-0005", "メールアドレスまたはパスワードに誤りがあります。正しく入力されているかご確認のうえ、再度お試しください。" },
            { "E-0006", "パスワードと確認用パスワードが一致していません。両方に同じパスワードを入力してください。" },

        public LoginController(LoginService loginService)
        {
            _loginService = loginService;
        }

        // 設計: SC-01 ログイン画面初期表示 (アクション名を Login に変更)
        [HttpGet]
        public IActionResult Login()
        {
            return View(new LoginViewModel());
        }

        // 設計: ログインボタン押下時の認証処理 (アクション名を Login に変更)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _loginService.AuthenticateAsync(model);

            if (!result.IsSuccess || result.Employee == null)
            {
                var errorCode = result.ErrorCode ?? "E-0005";
                model.ErrorMessage = ErrorMessages.TryGetValue(errorCode, out var message) ? message : errorCode;
                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, result.Employee.Id.ToString()),
                new Claim(ClaimTypes.Name, result.Employee.Name),
                new Claim("EmployeeNumber", result.Employee.EmployeeNumber),
                new Claim(ClaimTypes.Role, result.Employee.IsAdmin ? "Admin" : "User")
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity)
            );

            if (result.IsFirstLogin)
            {
                return RedirectToAction("Index", "FirstLogin");
            }

            // 打刻画面への遷移 (※打刻用HTML/View側のルーティング先に合わせて調整)
            return Redirect("/timestamp.html"); // または打刻用のViewルーティング
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }
    }
}