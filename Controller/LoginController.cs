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

        //エラーコードに対応する文言定義
        private static readonly Dictionary<string, string> ErrorMessages = new()
        {
            { "E-0001", "入力内容をご確認のうえ、再度入力してください。" },
            { "E-0002", "指定された桁数で入力してください。入力内容をご確認のうえ、再度入力してください。" },
            { "E-0003", "指定された文字種（英数字・記号等）で入力してください。入力内容をご確認のうえ、再度入力してください。" },
            { "E-0004", "パスワードは指定された形式（英大文字・小文字・数字を含む形式等）で入力してください。入力内容をご確認のうえ、再度入力してください。" },
            { "E-0005", "メールアドレスまたはパスワードに誤りがあります。正しく入力されているかご確認のうえ、再度お試しください。" },
            { "E-0006", "パスワードと確認用パスワードが一致していません。両方に同じパスワードを入力してください。" },
        };

        public LoginController(LoginService loginService)
        {
            _loginService = loginService;
        }

        //ログイン画面初期表示
        [HttpGet]
        public IActionResult Login()
        {
            return View(new LoginViewModel());
        }

        //ログインボタン押下時の認証処理
        [HttpPost]
        [ValidateAntiForgeryToken]   //攻撃を防ぐセキュリティトークンを検証
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _loginService.AuthenticateAsync(model);
            // 失敗した場合エラーを返す
            if (!result.IsSuccess || result.Employee == null)
            {
                var errorCode = result.ErrorCode ?? "E-0005";
                model.ErrorMessage = ErrorMessages.TryGetValue(errorCode, out var message) ? message : errorCode;
                return View(model);
            }

            // 認証に成功したユーザーの属性情報を Claim（身元証明の項目） として組み立て
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, result.Employee.Id.ToString()),
                new Claim(ClaimTypes.Name, result.Employee.Name),
                new Claim("EmployeeNumber", result.Employee.EmployeeNumber),
                new Claim(ClaimTypes.Role, result.Employee.IsAdmin ? "Admin" : "User")
            };

            //サインイン処理 (認証クッキー発行)
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity)
            );

            if (result.IsFirstLogin)
            {
                return RedirectToAction("Index", "FirstLogin");
            }

            //打刻画面への遷移
            return Redirect("/index.html"); 
        }

        //ログアウト処理
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }
    }
}