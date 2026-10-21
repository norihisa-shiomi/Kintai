using System;
using Xunit;
using Kintai.Services;

namespace Kintai.Tests
{
    public class AttendanceCalculatorTests
    {
        [Fact]
        public void Test_17Cases()
        {
            var cases = AttendanceTestData.Get17TestCases();

            foreach (var c in cases)
            {
                // string 型から DateTime / タプル型 への変換
                DateTime clockIn = DateTime.Parse(c.ClockIn);
                DateTime clockOut = DateTime.Parse(c.ClockOut);
                (DateTime Start, DateTime End)? breakStamp = ParseBreakStamp(c.ClockIn, c.BreakStamp);

                var res = AttendanceCalculator.Calculate(clockIn, clockOut, breakStamp, c.IsHoliday);

                Assert.True(c.ExpectedBreak == res.FormatTime(res.BreakTime), $"No.{c.No} [休憩時間] 期待値:{c.ExpectedBreak} 実測:{res.FormatTime(res.BreakTime)}");
                Assert.True(c.ExpectedWork == res.FormatTime(res.WorkHours), $"No.{c.No} [労働時間] 期待値:{c.ExpectedWork} 実測:{res.FormatTime(res.WorkHours)}");
                Assert.True(c.ExpectedOvertime == res.FormatTime(res.OvertimeHours), $"No.{c.No} [残業時間] 期待値:{c.ExpectedOvertime} 実測:{res.FormatTime(res.OvertimeHours)}");
                Assert.True(c.ExpectedMidnight == res.FormatTime(res.MidnightHours), $"No.{c.No} [深夜時間] 期待値:{c.ExpectedMidnight} 実測:{res.FormatTime(res.MidnightHours)}");
                Assert.True(c.ExpectedHolidayWork == res.FormatTime(res.HolidayWorkHours), $"No.{c.No} [休日労働時間] 期待値:{c.ExpectedHolidayWork} 実測:{res.FormatTime(res.HolidayWorkHours)}");
            }
        }

        // BreakStamp（"12:00-13:00" や "0:30" など）を (DateTime, DateTime)? に変換する処理
        private (DateTime Start, DateTime End)? ParseBreakStamp(string clockInStr, string breakStampStr)
        {
            if (string.IsNullOrWhiteSpace(breakStampStr)) return null;

            DateTime baseDate = DateTime.Parse(clockInStr).Date;

            if (breakStampStr.Contains("-"))
            {
                var parts = breakStampStr.Split('-');
                DateTime start = baseDate.Add(TimeSpan.Parse(parts[0]));
                DateTime end = baseDate.Add(TimeSpan.Parse(parts[1]));
                
                // 日またぎの休憩時間考慮（開始時刻より終了時刻が前の場合翌日扱い）
                if (end < start) end = end.AddDays(1);
                
                return (start, end);
            }
            else
            {
                // "0:30" などの時間長表記の場合（ダミーとして出勤時刻基準でセット）
                TimeSpan duration = TimeSpan.Parse(breakStampStr);
                DateTime start = baseDate;
                return (start, start.Add(duration));
            }
        }
    }
}