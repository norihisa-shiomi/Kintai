using System.ComponentModel.DataAnnotations;

namespace KintaiSystem.Models.Entities
{
    public class FirstLoginViewModel
    {
        /// メールアドレス
        [Required(ErrorMessage = "E-0001")]
        [EmailAddress(ErrorMessage = "E-0003")]
        [Display(Name = "メールアドレス")]
        public string Email { get; set; } = string.Empty;

        /// 初回登録用パスワード
        [Required(ErrorMessage = "E-0001")]
        [StringLength(32, MinimumLength = 8, ErrorMessage = "E-0002")]
        [DataType(DataType.Password)]
        [RegularExpression(@"^[a-zA-Z0-9!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]+$", ErrorMessage = "E-0003")]
        [Display(Name = "初回パスワード入力")]
        public string NewPassword { get; set; } = string.Empty;

        // 初回パスワード確認
        [Required(ErrorMessage = "E-0001")]
        [StringLength(32, MinimumLength = 8, ErrorMessage = "E-0002")]
        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.*[a-zA-Z])(?=.*\d)(?=.*[^\w\s]).+$", ErrorMessage = "E-0004")]
        [Compare(nameof(NewPassword), ErrorMessage = "E-0006")]
        [Display(Name = "初回パスワード確認")]
        public string ConfirmPassword { get; set; } = string.Empty;

        /// 画面表示用エラーメッセージ
        public string? ErrorMessage { get; set; }
    }
}