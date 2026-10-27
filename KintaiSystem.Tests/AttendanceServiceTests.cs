using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        [Fact(Skip = "締め機能の正式実装待ちのため一時スキップ")]
        public void 出勤_対象年月が締め完了済みの場合はE0016エラーとなること()
        {
            using (var context = GetInMemoryDbContext("Test_E0016"))
            {
                // context.DailyAttendances.Add(new DailyAttendance
                // {
                //     EmployeeId = 1,
                //     WorkDate = DateTime.Today,
                //     CurrentStatus = "勤務前",
                //     // IsClosed = true
                // });
                // context.SaveChanges();

                // var service = new AttendanceService(context);
                // var result = service.ProcessStamp(1, "出勤", DateTime.Now);

                // Assert.False(result.IsSuccess);
                // Assert.Contains("E-0016", result.Message);
            }
        }

        // --- SC-05 月次一覧 取得・集計テスト ---

        [Fact]
        public async Task 月次一覧_1ヶ月全日補完と月合計集計が正しく行われること()
        {
            using (var context = GetInMemoryDbContext("Test_MonthlyAttendance_Summary"))
            {
                // 社員登録 (SC-05 No.22 氏名表示確認用)
                context.Employees.Add(new Employee
                {
                    Id = 1,
                    EmployeeNumber = "E0001",
                    Name = "テスト 太郎",
                    Email = "test@example.com"
                });

                // 2026年10月1日（木・平日: 9:00〜18:00、休憩1h → 実働8:00、残業0:00）
                context.DailyAttendances.Add(new DailyAttendance
                {
                    EmployeeId = 1,
                    WorkDate = new DateTime(2026, 10, 1),
                    ClockInTime = new DateTime(2026, 10, 1, 9, 0, 0),
                    ClockOutTime = new DateTime(2026, 10, 1, 18, 0, 0),
                    CurrentStatus = "退勤済み",
                    BreakTimes = new List<BreakTime>
                    {
                        new BreakTime
                        {
                            BreakStartTime = new DateTime(2026, 10, 1, 12, 0, 0),
                            BreakEndTime = new DateTime(2026, 10, 1, 13, 0, 0)
                        }
                    }
                });

                // 2026年10月3日（土・休日: 9:00〜18:00、休憩1h → 休日労働8:00）
                context.DailyAttendances.Add(new DailyAttendance
                {
                    EmployeeId = 1,
                    WorkDate = new DateTime(2026, 10, 3),
                    ClockInTime = new DateTime(2026, 10, 3, 9, 0, 0),
                    ClockOutTime = new DateTime(2026, 10, 3, 18, 0, 0),
                    CurrentStatus = "退勤済み",
                    BreakTimes = new List<BreakTime>
                    {
                        new BreakTime
                        {
                            BreakStartTime = new DateTime(2026, 10, 3, 12, 0, 0),
                            BreakEndTime = new DateTime(2026, 10, 3, 13, 0, 0)
                        }
                    }
                });

                context.SaveChanges();

                var service = new AttendanceService(context);
                var result = await service.GetMonthlyAttendanceAsync(1, 2026, 10);

                // 1. 全日補完の検証: 10月は31日間すべての日付行が存在すること
                Assert.Equal(31, result.DailyList.Count);

                // 2. 出勤日数（平日1日、休日出勤1日）の検証 (設計書 6-3)
                Assert.Equal(1, result.WorkDays);
                Assert.Equal(1, result.HolidayWorkDays);

                // 3. 総労働時間（平日8h + 休日8h = 16:00）と休日労働（8:00）の検証
                Assert.Equal("16:00", result.TotalWorkHours);
                Assert.Equal("8:00", result.TotalHolidayWorkHours);

                // 4. 未出勤日（10月2日）が空行として補完されているかの検証
                var day2 = result.DailyList[1];
                Assert.Equal("10/02", day2.DisplayDate);
                Assert.Null(day2.ClockInTime);
                Assert.Equal("0:00", day2.WorkHours);
            }
        }

        [Fact(Skip = "締め機能の合流待ちのため一時スキップ")]
        public async Task 月次一覧_締め済みデータが存在する場合は締め状態となること()
        {
            using (var context = GetInMemoryDbContext("Test_MonthlyAttendance_LockStatus"))
            {
                context.Employees.Add(new Employee
                {
                    Id = 2,
                    EmployeeNumber = "E0002",
                    Name = "締めテスト 社員",
                    Email = "closed@example.com"
                });

                // 締め済み（IsClosed = true）の勤務データを登録
                context.DailyAttendances.Add(new DailyAttendance
                {
                    EmployeeId = 2,
                    WorkDate = new DateTime(2026, 9, 15),
                    ClockInTime = new DateTime(2026, 9, 15, 9, 0, 0),
                    ClockOutTime = new DateTime(2026, 9, 15, 18, 0, 0),
                    CurrentStatus = "退勤済み",
                    // IsClosed = true  // ← コンパイルエラーを防ぐためコメントアウト
                });

                context.SaveChanges();

                var service = new AttendanceService(context);
                var result = await service.GetMonthlyAttendanceAsync(2, 2026, 9);

                // 締め状態（ロック中）の判定検証
                Assert.True(result.IsClosed);
                Assert.Equal("締め済み（ロック）", result.LockStatus);
            }
        }
    }
}