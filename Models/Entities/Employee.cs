using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KintaiSystem.Models.Entities
{
    // テーブル定義: employees（社員）
    [Table("employees")]
    public class Employee
    {
        // テーブル定義 No.1: id (社員ID)
        // 主キー、システム内部で一意に識別するID
        [Key]
        [Column("id")]
        public int Id { get; set; }

        // テーブル定義 No.2: employee_number (社員番号)
        // 固有キー、英数字5桁、必須、画面表示・CSV出力用
        [Required]
        [MaxLength(5)]
        [Column("employee_number")]
        public string EmployeeNumber { get; set; } = string.Empty;

        // テーブル定義 No.3: name (氏名)
        // 必須、ログインユーザ名・画面表示用
        [Required]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        // テーブル定義 No.4: email (メールアドレス)
        // 固有キー、英数記号、必須、ログイン用メールアドレス (SC-01 / SC-02 入力項目)
        [Required]
        [EmailAddress]
        [Column("email")]
        public string Email { get; set; } = string.Empty;

        // テーブル定義 No.5: password_hash (パスワード)
        // 必須、ハッシュ化して保存 (SC-01 / SC-02 / SC-03)
        [Required]
        [Column("password_hash")]
        public string PasswordHash { get; set; } = string.Empty;

        // テーブル定義 No.6: is_admin (管理者フラグ)
        // 真偽値（Boolean）、必須、一般社員（0:偽）/ 管理者（1:真）（SC-04等制御用）
        [Required]
        [Column("is_admin")]
        public bool IsAdmin { get; set; } = false;

        // テーブル定義 No.7: is_first_login (初回ログインフラグ)
        // 真偽値（Boolean）、必須、「初回ログイン（パスワード初期設定）」を実現するためのフラグ (SC-02 制御用)
        [Required]
        [Column("is_first_login")]
        public bool IsFirstLogin { get; set; } = true;
    }
}