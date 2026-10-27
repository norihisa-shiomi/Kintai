using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using KintaiSystem.Data;
using KintaiSystem.Models.Entities;
using KintaiSystem.Services;

namespace KintaiSystem.Tests.Services
{
    public class AttendanceServiceMonthlySummaryTests
    {
        /// <summary>
        /// テストごとに独立した InMemory DbContext を生成するヘルパー
        /// </summary>
        private AppDbContext CreateInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        [Fact]
        public async Task GetOrCalculateMonthlySummaryAsync_勤務0日の社員も含め社員番号昇順で出力されること()
        {
            // Arrange
            using var context = CreateInMemoryDbContext();

            // 社員を意図的に番号逆順で登録（第1章 No.12: 社員番号昇順ソート要件の検証）
            context.Employees.AddRange(
                new Employee { Id = 1, EmployeeNumber = "EMP02", Name = "社員B", Email = "b@example.com", PasswordHash = "hash" },
                new Employee { Id = 2, EmployeeNumber = "EMP01", Name = "社員A", Email = "a@example.com", PasswordHash = "hash" }
            );

            // 社員Aのみ勤務打刻を登録（社員Bは勤務なし）
            var attendanceA = new DailyAttendance
            {
                Id = 1,
                EmployeeId = 2,
                WorkDate = new DateTime(2026, 10, 5), // 月曜日
                ClockInTime = new DateTime(2026, 10, 5, 9, 0, 0),
                ClockOutTime = new DateTime(2026, 10, 5, 18, 0, 0),
                BreakTimes = new List<BreakTime>
                {
                    new BreakTime
                    {
                        Id = 1,
                        BreakStartTime = new DateTime(2026, 10, 5, 12, 0, 0),
                        BreakEndTime = new DateTime(2026, 10, 5, 13, 0, 0)
                    }
                }
            };
            context.DailyAttendances.Add(attendanceA);
            await context.SaveChangesAsync();

            var service = new AttendanceService(context);

            // Act
            var result = await service.GetOrCalculateMonthlySummaryAsync("2026-10");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("2026-10", result.TargetYearMonth);
            Assert.Equal(2, result.TotalEmployeeCount);
            Assert.Equal(2, result.EmployeeSummaries.Count);

            // 1人目: EMP01（社員A）
            var row1 = result.EmployeeSummaries[0];
            Assert.Equal("EMP01", row1.EmployeeNumber);
            Assert.Equal("社員A", row1.EmployeeName);
            Assert.Equal(1, row1.WorkDays);
            Assert.Equal(0, row1.HolidayWorkDays);
            Assert.Equal("8:00", row1.TotalWorkHours);
            Assert.Equal("0:00", row1.TotalOvertimeHours);

            // 2人目: EMP02（社員B: 勤務実績0件）
            var row2 = result.EmployeeSummaries[1];
            Assert.Equal("EMP02", row2.EmployeeNumber);
            Assert.Equal("社員B", row2.EmployeeName);
            Assert.Equal(0, row2.WorkDays);
            Assert.Equal(0, row2.HolidayWorkDays);
            Assert.Equal("0:00", row2.TotalWorkHours);
            Assert.Equal("0:00", row2.TotalOvertimeHours);

            // 全体サマリー（SC-09 No.4）
            Assert.Equal("8:00", result.OverallTotalWorkHours);
            Assert.Equal("0:00", result.OverallTotalOvertimeHours);
        }

        [Fact]
        public async Task GetOrCalculateMonthlySummaryAsync_既存データがある場合は上書き更新されること()
        {
            // Arrange
            using var context = CreateInMemoryDbContext();

            var emp = new Employee { Id = 1, EmployeeNumber = "EMP01", Name = "社員A", Email = "a@example.com", PasswordHash = "hash" };
            context.Employees.Add(emp);

            // 既存の月次集計レコードを登録（過去の計算結果をシミュレート）
            context.MonthlySummaries.Add(new MonthlySummary
            {
                Id = 1,
                EmployeeId = 1,
                TargetYearMonth = "2026-10",
                WorkDays = 1,
                TotalWorkHours = "7:00",
                LockStatus = "未締め（編集可）"
            });

            // 新たに8時間勤務の打刻を追加
            context.DailyAttendances.Add(new DailyAttendance
            {
                Id = 1,
                EmployeeId = 1,
                WorkDate = new DateTime(2026, 10, 6),
                ClockInTime = new DateTime(2026, 10, 6, 9, 0, 0),
                ClockOutTime = new DateTime(2026, 10, 6, 18, 0, 0),
                BreakTimes = new List<BreakTime>
                {
                    new BreakTime
                    {
                        Id = 1,
                        BreakStartTime = new DateTime(2026, 10, 6, 12, 0, 0),
                        BreakEndTime = new DateTime(2026, 10, 6, 13, 0, 0)
                    }
                }
            });
            await context.SaveChangesAsync();

            var service = new AttendanceService(context);

            // Act
            await service.GetOrCalculateMonthlySummaryAsync("2026-10");

            // Assert: 重複して新規追加されず、既存のレコード（Id=1）が更新されていること
            var summaries = await context.MonthlySummaries.Where(m => m.TargetYearMonth == "2026-10").ToListAsync();
            Assert.Single(summaries);
            Assert.Equal(1, summaries[0].WorkDays);
            Assert.Equal("8:00", summaries[0].TotalWorkHours);
        }

        [Fact]
        public async Task GetOrCalculateMonthlySummaryAsync_法定休憩不足がある場合_警告フラグが立つこと()
        {
            // Arrange
            using var context = CreateInMemoryDbContext();

            var emp = new Employee { Id = 1, EmployeeNumber = "EMP01", Name = "社員A", Email = "a@example.com", PasswordHash = "hash" };
            context.Employees.Add(emp);

            // 拘束7時間・打刻休憩なし（外部設計書 第6章 6-2 No.3〜6: 法定休憩補正が発生するケース）
            context.DailyAttendances.Add(new DailyAttendance
            {
                Id = 1,
                EmployeeId = 1,
                WorkDate = new DateTime(2026, 10, 7),
                ClockInTime = new DateTime(2026, 10, 7, 9, 0, 0),
                ClockOutTime = new DateTime(2026, 10, 7, 16, 0, 0),
                BreakTimes = new List<BreakTime>() // 休憩打刻なし
            });
            await context.SaveChangesAsync();

            var service = new AttendanceService(context);

            // Act
            var result = await service.GetOrCalculateMonthlySummaryAsync("2026-10");

            // Assert: SC-09 No.6 補正警告アイコン用フラグが true になっていること
            var row = Assert.Single(result.EmployeeSummaries);
            Assert.True(row.HasBreakAdjustmentWarning);
        }
    }
}