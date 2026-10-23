using System.ComponentModel.DataAnnotations;

namespace YourProjectName.Models.ViewModels
{
    /// <summary>
    /// SC-02 初回ログイン画面用 ViewModel
    /// </summary>
    public class FirstLoginViewModel
    {
        // -------------------------------------------------------------------
        // No 1: メールアドレス (入力 / 必須)
        // -------------------------------------------------------------------
        /// <summary>
        /// メールアドレス
        /// 設計ルール (SC-02 No.1):
        /// - 必須チェック: E-0001
        /// - 文字種・形式チェック: E-0003
        /// </summary>
        [Required(ErrorMessage = "E-0001")]
        [EmailAddress(ErrorMessage = "E-0003")]
        [Display(Name = "メールアドレス")]
        public string Email { get; set; } = string.Empty;

        // -------------------------------------------------------------------
        // No 2: 初回パスワード入力 (入力 / 必須 / 8〜32桁 / 英数記号)
        // -------------------------------------------------------------------
        /// <summary>
        /// 初回登録用パスワード
        /// 設計ルール (SC-02 No.2):
        /// - 必須チェック: E-0001
        /// - 桁数チェック (8〜32桁): E-0002
        /// - 文字種チェック (英数記号): E-0003
        /// </summary>
        [Required(ErrorMessage = "E-0001")]
        [StringLength(32, MinimumLength = 8, ErrorMessage = "E-0002")]
        [DataType(DataType.Password)]
        // 正規表現: 英文字・数字・記号で構成される文字列を許可
        [RegularExpression(@"^[a-zA-Z0-9!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]+$", ErrorMessage = "E-0003")]
        [Display(Name = "初回パスワード入力")]
        public string NewPassword { get; set; } = string.Empty;

        // -------------------------------------------------------------------
        // No 3: 初回パスワード確認 (入力 / 必須 / 8〜32桁 / 英数記号各1文字以上 / 相関一致)
        // -------------------------------------------------------------------
        /// <summary>
        /// 確認用パスワード
        /// 設計ルール (SC-02 No.3):
        /// - 必須チェック: E-0001
        /// - 桁数チェック (8〜32桁): E-0002
        /// - 文字種チェック (英数記号各1文字以上): E-0004
        /// - 相関一致チェック (NewPassword との一致): E-0006
        /// </summary>
        [Required(ErrorMessage = "E-0001")]
        [StringLength(32, MinimumLength = 8, ErrorMessage = "E-0002")]
        [DataType(DataType.Password)]
        // 正規表現: 英字(?=.*[a-zA-Z])、数字(?=.*\d)、記号(?=.*[^\w\s])がそれぞれ1文字以上含まれているかを検証
        [RegularExpression(@"^(?=.*[a-zA-Z])(?=.*\d)(?=.*[^\w\s]).+$", ErrorMessage = "E-0004")]
        [Compare(nameof(NewPassword), ErrorMessage = "E-0006")]
        [Display(Name = "初回パスワード確認")]
        public string ConfirmPassword { get; set; } = string.Empty;

        // -------------------------------------------------------------------
        // No 5: エラーメッセージ (表示)
        // -------------------------------------------------------------------
        /// <summary>
        /// 画面表示用エラーメッセージ
        /// 設計ルール (SC-02 No.5):
        /// 業務エラー（E-0005: 登録対象外メールアドレス、E-0006: 不一致等）発生時にコントローラーから設定されるエラー情報
        /// </summary>
        public string? ErrorMessage { get; set; }
    }
}