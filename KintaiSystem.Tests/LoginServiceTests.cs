using System;
using System.Linq;
using KintaiSystem.Data;
using Microsoft.EntityFrameworkCore;
using KintaiSystem.Models.Entities;
using Microsoft.AspNetCore.Identity;
using KintaiSystem.Services;
using Xunit;

namespace KintaiSystem.Tests.Services
{
    public class LoginServiceTests
    {
        /// <summary>
        /// テストごとに独立したインメモリDbContextを作成するヘルパー
        /// </summary>
        private AppDbContext CreateInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        #region 正常系テスト

        [Fact]
        public async Task AuthenticateAsync_認証成功_通常社員_成功結果と社員情報が返却されること()
        {
            // Arrange
            using var context = CreateInMemoryDbContext();
            var targetEmployee = new Employee
            {
                Id = 1,
                EmployeeNumber = "10001",
                Name = "山田太郎",
                Email = "test@example.com",
                PasswordHash = "HashedPassword123!",
                IsAdmin = false,
                IsFirstLogin = false
            };
            context.Employees.Add(targetEmployee);
            await context.SaveChangesAsync();

            // Moqを使わずスタブクラスを使用（認証成功を返す）
            var stubHasher = new StubPasswordHasher(PasswordVerificationResult.Success);
            var service = new LoginService(context, stubHasher);

            var model = new LoginViewModel
            {
                Email = "test@example.com",
                Password = "Password123!"
            };

            // Act
            var result = await service.AuthenticateAsync(model);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.False(result.IsFirstLogin);
            Assert.Null(result.ErrorCode);
            Assert.NotNull(result.Employee);
            Assert.Equal("10001", result.Employee.EmployeeNumber);
        }

        [Fact]
        public async Task AuthenticateAsync_認証成功_初回ログイン_IsFirstLoginがtrueで返却されること()
        {
            // Arrange
            using var context = CreateInMemoryDbContext();
            var targetEmployee = new Employee
            {
                Id = 2,
                EmployeeNumber = "10002",
                Name = "佐藤花子",
                Email = "first_login@example.com",
                PasswordHash = "InitialHashedPassword!",
                IsAdmin = false,
                IsFirstLogin = true
            };
            context.Employees.Add(targetEmployee);
            await context.SaveChangesAsync();

            var stubHasher = new StubPasswordHasher(PasswordVerificationResult.Success);
            var service = new LoginService(context, stubHasher);

            var model = new LoginViewModel
            {
                Email = "first_login@example.com",
                Password = "InitialPass123!"
            };

            // Act
            var result = await service.AuthenticateAsync(model);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.True(result.IsFirstLogin);
            Assert.Null(result.ErrorCode);
        }

        #endregion

        #region 異常系テスト (入力不備・E-0001)

        [Theory]
        [InlineData("", "Password123!")]
        [InlineData("test@example.com", "")]
        [InlineData("", "")]
        [InlineData(null, "Password123!")]
        [InlineData("test@example.com", null)]
        public async Task AuthenticateAsync_必須項目未入力_E0001エラーが返却されること(string? email, string? password)
        {
            // Arrange
            using var context = CreateInMemoryDbContext();
            var stubHasher = new StubPasswordHasher(PasswordVerificationResult.Success);
            var service = new LoginService(context, stubHasher);

            var model = new LoginViewModel
            {
                Email = email!,
                Password = password!
            };

            // Act
            var result = await service.AuthenticateAsync(model);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("E-0001", result.ErrorCode);
            Assert.Null(result.Employee);
        }

        [Fact]
        public async Task AuthenticateAsync_モデル自体がnull_E0001エラーが返却されること()
        {
            // Arrange
            using var context = CreateInMemoryDbContext();
            var stubHasher = new StubPasswordHasher(PasswordVerificationResult.Success);
            var service = new LoginService(context, stubHasher);

            // Act
            var result = await service.AuthenticateAsync(null!);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("E-0001", result.ErrorCode);
        }

        #endregion

        #region 異常系テスト (業務ルール・一致しない・E-0005)

        [Fact]
        public async Task AuthenticateAsync_該当メールアドレスが存在しない_E0005エラーが返却されること()
        {
            // Arrange
            using var context = CreateInMemoryDbContext();
            var stubHasher = new StubPasswordHasher(PasswordVerificationResult.Success);
            var service = new LoginService(context, stubHasher);

            var model = new LoginViewModel
            {
                Email = "notfound@example.com",
                Password = "Password123!"
            };

            // Act
            var result = await service.AuthenticateAsync(model);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("E-0005", result.ErrorCode);
            Assert.Null(result.Employee);
        }

        [Fact]
        public async Task AuthenticateAsync_パスワード不一致_E0005エラーが返却されること()
        {
            // Arrange
            using var context = CreateInMemoryDbContext();
            var targetEmployee = new Employee
            {
                Id = 1,
                EmployeeNumber = "10001",
                Name = "山田太郎",
                Email = "test@example.com",
                PasswordHash = "CorrectHashedPassword!",
                IsAdmin = false,
                IsFirstLogin = false
            };
            context.Employees.Add(targetEmployee);
            await context.SaveChangesAsync();

            // パスワード検証失敗 (Failed) を返すスタブを設定
            var stubHasher = new StubPasswordHasher(PasswordVerificationResult.Failed);
            var service = new LoginService(context, stubHasher);

            var model = new LoginViewModel
            {
                Email = "test@example.com",
                Password = "WrongPassword123!"
            };

            // Act
            var result = await service.AuthenticateAsync(model);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("E-0005", result.ErrorCode);
            Assert.Null(result.Employee);
        }

        #endregion

        #region テスト用スタブクラス (Moqライブラリの代替)

        /// <summary>
        /// Moqを使わずにパスワード照合結果を擬似的に制御するスタブ
        /// </summary>
        private class StubPasswordHasher : IPasswordHasher<Employee>
        {
            private readonly PasswordVerificationResult _result;

            public StubPasswordHasher(PasswordVerificationResult result)
            {
                _result = result;
            }

            public PasswordVerificationResult VerifyHashedPassword(Employee user, string hashedPassword, string providedPassword)
            {
                return _result;
            }

            public string HashPassword(Employee user, string password)
            {
                return "dummy_hash";
            }
        }

        #endregion
    }
}