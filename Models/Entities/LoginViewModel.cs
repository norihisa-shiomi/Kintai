using System.ComponentModel.DataAnnotations;

namespace YourProjectName.Models
{
    /// <summary>
    /// SC-01 ログイン画面用 ViewModel
    /// </summary>
    public class LoginViewModel
    {
        // -------------------------------------------------------------------
        // No 1: メールアドレス
        // -------------------------------------------------------------------
        /// <summary>
        /// ログイン用メールアドレス
        /// 設計ルール:
        /// - 必須チェック: E-0001
        /// - 文字種・形式チェック: E-0003
        /// </summary>
        [Required(ErrorMessage = "E-0001")]
        [EmailAddress(ErrorMessage = "E-0003")]
        [Display(Name = "メールアドレス")]
        public string Email { get; set; } = string.Empty;

        // -------------------------------------------------------------------
        // No 2: パスワード
        // -------------------------------------------------------------------
        /// <summary>
        /// パスワード
        /// 設計ルール:
        /// - 必須チェック: E-0001
        /// - 桁数チェック (8〜32桁): E-0002
        /// - 文字種チェック (英数記号各1文字以上): E-0004
        /// </summary>
        [Required(ErrorMessage = "E-0001")]
        [StringLength(32, MinimumLength = 8, ErrorMessage = "E-0002")]
        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.*[a-zA-Z])(?=.*\d)(?=.*[^\w\s]).+$", ErrorMessage = "E-0004")]
        [Display(Name = "パスワード")]
        public string Password { get; set; } = string.Empty;

        // -------------------------------------------------------------------
        // No 4: エラーメッセージ (画面表示用)
        // -------------------------------------------------------------------
        /// <summary>
        /// 認証失敗時（E-0005等）に画面上に表示するエラーメッセージID、または変換後のメッセージを保持
        /// 設計: SC-01 No.4 エラーメッセージ
        /// </summary>
        public string? ErrorMessage { get; set; }
    }
}