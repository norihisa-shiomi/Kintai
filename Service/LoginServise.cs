using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using KintaiSystem.Models.Entities;
using YourProjectName.Models;

namespace KintaiSystem.Services
{
    // 認証結果をコントローラーへ返却するための結果クラス
    public class LoginResult
    {
        // ログイン成功フラグ
        public bool IsSuccess { get; set; }

        // 初回ログインフラグ (SC-02へのリダイレクト判定用)
        public bool IsFirstLogin { get; set; }

        // 画面表示用エラーコード (E-0005等)
        public string? ErrorCode { get; set; }

        // ログイン成功時の社員オブジェクト
        public Employee? Employee { get; set; }
    }

    public class LoginService
    {
        // データベースコンテキスト (依存性注入)
        private readonly AppDbContext _context;

        // ASP.NET Core 8.0 標準のパスワードハッシュ検証インターフェース
        private readonly IPasswordHasher<Employee> _passwordHasher;

        public LoginService(AppDbContext context, IPasswordHasher<Employee> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        /// <summary>
        /// ログイン処理メインエントリーポイント 
        /// </summary>
        public async Task<LoginResult> AuthenticateAsync(LoginViewModel model)
        {
            // 1. 入力値検証 
            if (model == null || string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Password))
            {
                return new LoginResult { IsSuccess = false, ErrorCode = "E-0001" };
            }

            // 2. メールアドレスによる対象社員のDB検索 
            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.Email == model.Email);

            // 3. 該当ユーザー不在時の判定
            if (employee == null)
            {
                return new LoginResult { IsSuccess = false, ErrorCode = "E-0005" };
            }

            // 4. パスワードのハッシュ照合
            var verificationResult = _passwordHasher.VerifyHashedPassword(
                employee, 
                employee.PasswordHash, 
                model.Password
            );

            // 5. パスワード不一致時の判定 
            if (verificationResult == PasswordVerificationResult.Failed)
            {
                return new LoginResult { IsSuccess = false, ErrorCode = "E-0005" };
            }

            // 6. 認証成功処理および初回ログインフラグの評価 
            return new LoginResult
            {
                IsSuccess = true,
                IsFirstLogin = employee.IsFirstLogin,
                Employee = employee
            };
        }
    }
}