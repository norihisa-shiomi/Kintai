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

        // 設計: エラーコードに対応する文言定義辞書
        private static readonly Dictionary<string, string> ErrorMessages = new()
        {
            { "E-0001", "入力内容をご確認のうえ、再度入力してください。" },
            { "E-0002", "指定された桁数で入力してください。入力内容をご確認のうえ、再度入力してください。" },
            { "E-0003", "指定された文字種（英数字・記号等）で入力してください。入力内容をご確認のうえ、再度入力してください。" },
            { "E-0004", "パスワードは指定された形式（英大文字・小文字・数字を含む形式等）で入力してください。入力内容をご確認のうえ、再度入力してください。" },
            { "E-0005", "メールアドレスまたはパスワードに誤りがあります。正しく入力されているかご確認のうえ、再度お試しください。" },
            { "E-0006", "パスワードと確認用パスワードが一致していません。両方に同じパスワードを入力してください。" },
            { "E-0007", "初回パスワードが登録されていません。管理者へお問い合わせのうえ、初期パスワードの送付を受けてください。" }
        };

        // 設計: 依存性注入 (LoginService)
        public LoginController(LoginService loginService)
        {
            _loginService = loginService;
        }

        // 設計: SC-01 ログイン画面初期表示
        [HttpGet]
        public IActionResult Index()
        {
            return View(new LoginViewModel());
        }

        // 設計: ログインボタン押下時の認証処理
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(LoginViewModel model)
        {
            // 設計: 5. 入力チェック (アノテーション検証失敗時の再表示)
            if (!ModelState.IsValid)
            {
                // 設計: 3. 画面遷移 (SC-01 ログイン失敗 ⇒ SC-01 再表示)
                return View(model);
            }

            // 設計: 5-2. 状態のチェック (メールアドレス・パスワード合致検証)
            var result = await _loginService.AuthenticateAsync(model);

            // 設計: 3. 画面遷移 (SC-01 認証失敗 E-0005 ⇒ SC-01 エラー表示)
            if (!result.IsSuccess || result.Employee == null)
            {
                var errorCode = result.ErrorCode ?? "E-0005";
                model.ErrorMessage = ErrorMessages.TryGetValue(errorCode, out var message) ? message : errorCode;
                return View(model);
            }

            // 設計: クッキー認証用クレーム情報の生成
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, result.Employee.Id.ToString()),
                new Claim(ClaimTypes.Name, result.Employee.Name),
                new Claim("EmployeeNumber", result.Employee.EmployeeNumber),
                new Claim(ClaimTypes.Role, result.Employee.IsAdmin ? "Admin" : "User")
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            // 設計: サインイン処理 (暗号化クッキーの発行)
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity)
            );

            // 設計: 3. 画面遷移 (初回ログイン判定: IsFirstLogin == true ⇒ SC-02 初回ログイン画面)
            if (result.IsFirstLogin)
            {
                // ※初回ログイン画面のコントローラー作成後に名称を合わせてください
                return RedirectToAction("Index", "FirstLogin");
            }

            // 設計: 3. 画面遷移 (SC-01 ログイン成功 ⇒ SC-04 打刻画面: TimeStampController.Index)
            return RedirectToAction("Index", "TimeStamp");
        }

        // 設計: ログアウト処理 (クッキー破棄とログイン画面リダイレクト)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Index));
        }
    }
}