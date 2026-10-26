using System.ComponentModel.DataAnnotations;

namespace KintaiSystem.Models.Entities
{
    public class LoginViewModel
    {
        /// ログイン用メールアドレス
        [Required(ErrorMessage = "E-0001")]
        [EmailAddress(ErrorMessage = "E-0003")]
        [Display(Name = "メールアドレス")]
        public string Email { get; set; } = string.Empty;

        /// パスワード
        [Required(ErrorMessage = "E-0001")]
        [StringLength(32, MinimumLength = 8, ErrorMessage = "E-0002")]
        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.*[a-zA-Z])(?=.*\d)(?=.*[^\w\s]).+$", ErrorMessage = "E-0004")]
        [Display(Name = "パスワード")]
        public string Password { get; set; } = string.Empty;

        /// エラーメッセージ (画面表示用)
        public string? ErrorMessage { get; set; }
    }
}