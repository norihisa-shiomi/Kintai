using System.ComponentModel.DataAnnotations;

namespace KintaiSystem.Models.Entities
{
    public class Employee
    {
        // 主キー
        public int Id { get; set; }

        // 社員番号（英数字5桁）
        [StringLength(5)]
        public string EmployeeNumber { get; set; } = string.Empty;

        // 氏名
        public string Name { get; set; } = string.Empty;

        // メールアドレス
        public string Email { get; set; } = string.Empty;

        // パスワード（ハッシュ値）
        public string PasswordHash { get; set; } = string.Empty;

        // 管理者フラグ (一般社員: false / 管理者: true)
        public bool IsAdmin { get; set; } = false;

        // 初回ログインフラグ
        // origin/feature/stamps
        public bool IsFirstLogin { get; set; } = true;
    }
}