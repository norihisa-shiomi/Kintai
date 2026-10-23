using System;
using System.Collections.Generic;
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

            foreach (var testCase in cases)
            {
                DateTime clockIn = ParseDateTime(testCase.Date, testCase.ClockIn);
                DateTime clockOut = ParseDateTime(testCase.Date, testCase.ClockOut);
                var breakStamp = ParseBreakStamp(testCase.Date, testCase.ClockIn, testCase.BreakStamp);

                var result = AttendanceCalculator.Calculate(clockIn, clockOut, breakStamp, testCase.IsHoliday);

                Assert.True(testCase.ExpectedBreak == result.FormatTime(result.BreakTime), 
                    $"No.{testCase.No} 休憩時間不一致: 期待値={testCase.ExpectedBreak}, 実際={result.FormatTime(result.BreakTime)}");
                Assert.True(testCase.ExpectedWork == result.FormatTime(result.WorkHours), 
                    $"No.{testCase.No} 労働時間不一致: 期待値={testCase.ExpectedWork}, 実際={result.FormatTime(result.WorkHours)}");
                Assert.True(testCase.ExpectedOvertime == result.FormatTime(result.OvertimeHours), 
                    $"No.{testCase.No} 残業時間不一致: 期待値={testCase.ExpectedOvertime}, 実際={result.FormatTime(result.OvertimeHours)}");
                Assert.True(testCase.ExpectedMidnight == result.FormatTime(result.MidnightHours), 
                    $"No.{testCase.No} 深夜時間不一致: 期待値={testCase.ExpectedMidnight}, 実際={result.FormatTime(result.MidnightHours)}");
                Assert.True(testCase.ExpectedHolidayWork == result.FormatTime(result.HolidayWorkHours), 
                    $"No.{testCase.No} 休日労働不一致: 期待値={testCase.ExpectedHolidayWork}, 実際={result.FormatTime(result.HolidayWorkHours)}");
            }
        }

        private DateTime ParseDateTime(string dateStr, string timeStr)
        {
            DateTime baseDate = DateTime.Parse(dateStr);
            if (timeStr.StartsWith("翌"))
            {
                timeStr = timeStr.Replace("翌", "");
                baseDate = baseDate.AddDays(1);
            }
            TimeSpan time = TimeSpan.Parse(timeStr);
            return baseDate.Add(time);
        }

        private (DateTime Start, DateTime End)? ParseBreakStamp(string dateStr, string clockInStr, string breakStampStr)
        {
            if (string.IsNullOrEmpty(breakStampStr) || breakStampStr == "なし")
            {
                return null;
            }

            DateTime baseDate = DateTime.Parse(dateStr);

            // "12:00〜13:00" または "12:00~13:00" の形式
            if (breakStampStr.Contains("〜") || breakStampStr.Contains("~"))
            {
                string[] parts = breakStampStr.Split(new[] { '〜', '~' }, StringSplitOptions.RemoveEmptyEntries);
                DateTime start = baseDate.Add(TimeSpan.Parse(parts[0].Trim()));
                DateTime end = baseDate.Add(TimeSpan.Parse(parts[1].Trim()));

                // 打刻出勤より前の時間帯（例: 01:00〜02:00）は翌日扱いにする
                DateTime clockIn = ParseDateTime(dateStr, clockInStr);
                if (start < clockIn)
                {
                    start = start.AddDays(1);
                    end = end.AddDays(1);
                }
                return (start, end);
            }

            // "0:30" などの時間量直接指定の場合
            TimeSpan duration = TimeSpan.Parse(breakStampStr);
            DateTime defaultStart = ParseDateTime(dateStr, clockInStr).AddHours(4); // 仮の開始時刻
            return (defaultStart, defaultStart.Add(duration));
        }
    }
}