using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using KintaiSystem.Controllers;
using KintaiSystem.Models.Dtos;
using KintaiSystem.Services.Interfaces;

namespace KintaiSystem.Tests.Controllers
{
    public class MonthlySummaryControllerTests
    {
        // ---------------------------------------------------------------------
        // テスト用のダミーサービス（外部ライブラリ不要）
        // ---------------------------------------------------------------------
        private class FakeAttendanceService : IAttendanceService
        {
            public bool WasCalled { get; private set; }
            public string? LastTargetYearMonth { get; private set; }

            public Task<MonthlyAttendanceResponseDto> GetMonthlyAttendanceAsync(int employeeId, int year, int month)
            {
                throw new NotImplementedException();
            }

            public Task<MonthlySummaryResponseDto> GetOrCalculateMonthlySummaryAsync(string targetYearMonth)
            {
                WasCalled = true;
                LastTargetYearMonth = targetYearMonth;
                return Task.FromResult(new MonthlySummaryResponseDto
                {
                    TargetYearMonth = targetYearMonth,
                    TotalEmployeeCount = 10,
                    OverallTotalWorkHours = "1600:00"
                });
            }
        }

        private MonthlySummaryController CreateController(FakeAttendanceService service, string? isAdminValue)
        {
            var controller = new MonthlySummaryController(service);

            var claims = new System.Collections.Generic.List<Claim>
            {
                new Claim("EmployeeNumber", "EMP001")
            };

            if (isAdminValue != null)
            {
                claims.Add(new Claim("IsAdmin", isAdminValue));
            }

            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            return controller;
        }

        // 1. 一般社員アクセス -> 403 / E-0022
        [Fact]
        public async Task GetMonthlySummary_一般社員がアクセスした時_403ForbiddenとE0022を返すこと()
        {
            var service = new FakeAttendanceService();
            var controller = CreateController(service, "False");

            var result = await controller.GetMonthlySummary("2026-10");

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(403, objectResult.StatusCode);
            Assert.False(service.WasCalled); // サービスが呼ばれていないこと
        }

        // 2. フォーマット不正 -> 400 / E-0018
        [Theory]
        [InlineData("2026/10")]
        [InlineData("2026-1")]
        [InlineData("2026-13")]
        [InlineData("2026-00")]
        [InlineData("abcdefg")]
        public async Task GetMonthlySummary_年月フォーマットが不正な時_400BadRequestとE0018を返すこと(string invalidYearMonth)
        {
            var service = new FakeAttendanceService();
            var controller = CreateController(service, "True");

            var result = await controller.GetMonthlySummary(invalidYearMonth);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
            Assert.False(service.WasCalled);
        }

        // 3. 年月未指定 -> 当月補正で 200 OK
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GetMonthlySummary_年月未指定の時_当月を採用して200OKを返すこと(string? emptyYearMonth)
        {
            var service = new FakeAttendanceService();
            var controller = CreateController(service, "True");
            string currentYearMonth = DateTime.Now.ToString("yyyy-MM");

            var result = await controller.GetMonthlySummary(emptyYearMonth);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.True(service.WasCalled);
            Assert.Equal(currentYearMonth, service.LastTargetYearMonth);
        }

        // 4. 正常系 -> 200 OK
        [Fact]
        public async Task GetMonthlySummary_管理者が正しい年月を指定した時_200OKとDTOを返すこと()
        {
            var service = new FakeAttendanceService();
            var controller = CreateController(service, "True");

            var result = await controller.GetMonthlySummary("2026-10");

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);

            var responseData = Assert.IsType<MonthlySummaryResponseDto>(okResult.Value);
            Assert.Equal("2026-10", responseData.TargetYearMonth);
            Assert.True(service.WasCalled);
        }
    }
}