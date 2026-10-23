using System;
using System.Linq;
using KintaiSystem.Data;
using KintaiSystem.Models.Entities;
using KintaiSystem.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace KintaiSystem.Tests
{
    public class AttendanceServiceTests
    {
        private AppDbContext GetInMemoryDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            return new AppDbContext(options);
        }

        // --- 正常系 ---
        [Fact]
        public void 出勤_勤務前の場合は正常終了すること()
        {
            using (var context = GetInMemoryDbContext("Test_Success_ClockIn"))
            {
                var service = new AttendanceService(context);
                var result = service.ProcessStamp(1, "出勤", DateTime.Now);

                Assert.True(result.IsSuccess);
            }
        }

        // --- 異常系 (仕様書 5-2 準拠 7ケース) ---

        [Fact]
        public void 出勤_勤務中または休憩中の場合はE0010エラーとなること()
        {
            using (var context = GetInMemoryDbContext("Test_E0010"))
            {
                context.DailyAttendances.Add(new DailyAttendance
                {
                    EmployeeId = 1,
                    WorkDate = DateTime.Today,
                    CurrentStatus = "勤務中"
                });
                context.SaveChanges();

                var service = new AttendanceService(context);
                var result = service.ProcessStamp(1, "出勤", DateTime.Now);

                Assert.False(result.IsSuccess);
                Assert.Contains("E-0010", result.Message);
            }
        }

        [Fact]
        public void 出勤_前回の退勤が未完了の場合はE0011エラーとなること()
        {
            using (var context = GetInMemoryDbContext("Test_E0011"))
            {
                // 前日のデータで出勤のみがあり、退勤がない（ClockOutTimeがnull）状態を作る
                context.DailyAttendances.Add(new DailyAttendance
                {
                    EmployeeId = 1,
                    WorkDate = DateTime.Today.AddDays(-1),
                    ClockInTime = DateTime.Now.AddDays(-1),
                    ClockOutTime = null,
                    CurrentStatus = "勤務前" // 画面上はリセットされたが未退勤データが残っている想定
                });
                context.SaveChanges();

                var service = new AttendanceService(context);
                var result = service.ProcessStamp(1, "出勤", DateTime.Now);

                Assert.False(result.IsSuccess);
                Assert.Contains("E-0011", result.Message);
            }
        }

        [Fact]
        public void 退勤_勤務前または退勤済みまたは休憩中の場合はE0012エラーとなること()
        {
            using (var context = GetInMemoryDbContext("Test_E0012"))
            {
                context.DailyAttendances.Add(new DailyAttendance
                {
                    EmployeeId = 1,
                    WorkDate = DateTime.Today,
                    CurrentStatus = "勤務前"
                });
                context.SaveChanges();

                var service = new AttendanceService(context);
                var result = service.ProcessStamp(1, "退勤", DateTime.Now);

                Assert.False(result.IsSuccess);
                Assert.Contains("E-0012", result.Message);
            }
        }

        [Fact]
        public void 退勤_出勤時刻から24時間以上経過している場合はE0013エラーとなること()
        {
            using (var context = GetInMemoryDbContext("Test_E0013"))
            {
                var now = DateTime.Now;
                // 25時間前に出勤したデータを作成
                context.DailyAttendances.Add(new DailyAttendance
                {
                    EmployeeId = 1,
                    WorkDate = now.AddHours(-25).Date,
                    ClockInTime = now.AddHours(-25),
                    CurrentStatus = "勤務中"
                });
                context.SaveChanges();

                var service = new AttendanceService(context);
                var result = service.ProcessStamp(1, "退勤", now);

                Assert.False(result.IsSuccess);
                Assert.Contains("E-0013", result.Message);
            }
        }

        [Fact]
        public void 休憩開始_勤務中以外の場合はE0014エラーとなること()
        {
            using (var context = GetInMemoryDbContext("Test_E0014"))
            {
                context.DailyAttendances.Add(new DailyAttendance
                {
                    EmployeeId = 1,
                    WorkDate = DateTime.Today,
                    CurrentStatus = "休憩中"
                });
                context.SaveChanges();

                var service = new AttendanceService(context);
                var result = service.ProcessStamp(1, "休憩開始", DateTime.Now);

                Assert.False(result.IsSuccess);
                Assert.Contains("E-0014", result.Message);
            }
        }

        [Fact]
        public void 休憩終了_休憩中以外の場合はE0015エラーとなること()
        {
            using (var context = GetInMemoryDbContext("Test_E0015"))
            {
                context.DailyAttendances.Add(new DailyAttendance
                {
                    EmployeeId = 1,
                    WorkDate = DateTime.Today,
                    CurrentStatus = "勤務中"
                });
                context.SaveChanges();

                var service = new AttendanceService(context);
                var result = service.ProcessStamp(1, "休憩終了", DateTime.Now);

                Assert.False(result.IsSuccess);
                Assert.Contains("E-0015", result.Message);
            }
        }

        [Fact]
        public void 出勤_対象年月が締め完了済みの場合はE0016エラーとなること()
        {
            using (var context = GetInMemoryDbContext("Test_E0016"))
            {
                // 当月を締め完了（IsClosed = true）状態として登録しておく設定（MonthlyAttendance等）
                context.DailyAttendances.Add(new DailyAttendance
                {
                    EmployeeId = 1,
                    WorkDate = DateTime.Today,
                    CurrentStatus = "勤務前",
                    IsClosed = true // 締めフラグが立っている
                });
                context.SaveChanges();

                var service = new AttendanceService(context);
                var result = service.ProcessStamp(1, "出勤", DateTime.Now);

                Assert.False(result.IsSuccess);
                Assert.Contains("E-0016", result.Message);
            }
        }
    }
}