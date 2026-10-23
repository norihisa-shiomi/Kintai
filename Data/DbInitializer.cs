using System.Linq;
using KintaiSystem.Models.Entities;

namespace KintaiSystem.Data
{
    public static class DbInitializer
    {
        public static void Initialize(AppDbContext context)
        {
            // データベースが存在しない場合は作成
            context.Database.EnsureCreated();

            // 社員データがすでに存在する場合は何もしない
            if (context.Employees.Any())
            {
                return;
            }

            // テスト用の社員データを登録（設計書通りの項目で設定）
            var testEmployee = new Employee
            {
                EmployeeNumber = "E0001",
                Name = "山田 太郎",
                Email = "yamada@example.com",
                PasswordHash = "Password123!", // テスト用仮パスワード
                IsAdmin = false,
                IsFirstLogin = false
            };

            context.Employees.Add(testEmployee);
            context.SaveChanges();
        }
    }
}