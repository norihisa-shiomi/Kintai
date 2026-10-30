using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;
using Kintai.Services;

namespace Kintai.Tests
{
    public class AcceptanceTest
    {
        private readonly ITestOutputHelper _output;

        // コンストラクタで出力ヘルパーを受け取る
        public AcceptanceTest(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void Test_K01_to_K37_AcceptanceCases()
        {
            var cases = AcceptanceTestData.Get37TestCases();

            foreach (var testCase in cases)
            {
                DateTime clockIn = ParseDateTime(testCase.Date, testCase.ClockIn);
                DateTime clockOut = ParseDateTime(testCase.Date, testCase.ClockOut);
                
                if (clockOut <= clockIn)
                {
                    clockOut = clockOut.AddDays(1);
                }

                var breakStamps = ParseBreakStamps(testCase.Date, clockIn, testCase.BreakStamp);

                var result = AttendanceCalculator.Calculate(clockIn, clockOut, breakStamps, testCase.IsHoliday);

                string actBreak = result.FormatTime(result.BreakTime);
                string actWork = result.FormatTime(result.WorkHours);
                string actOvertime = result.FormatTime(result.OvertimeHours);
                string actMidnight = result.FormatTime(result.MidnightHours);
                string actHoliday = result.FormatTime(result.HolidayWorkHours);

                // 各ケースの期待値と計算値をターミナルに出力
                _output.WriteLine(string.Format(
                    "[{0}] 休憩:{1}(exp:{2}) | 労働:{3}(exp:{4}) | 残業:{5}(exp:{6}) | 深夜:{7}(exp:{8}) | 休日:{9}(exp:{10})",
                    testCase.No,
                    actBreak, testCase.ExpectedBreak,
                    actWork, testCase.ExpectedWork,
                    actOvertime, testCase.ExpectedOvertime,
                    actMidnight, testCase.ExpectedMidnight,
                    actHoliday, testCase.ExpectedHolidayWork));

                // アサートによる完全一致判定
                Assert.True(testCase.ExpectedBreak == actBreak, 
                    $"[{testCase.No}] 休憩時間不一致: 期待値={testCase.ExpectedBreak}, 実際={actBreak}");
                Assert.True(testCase.ExpectedWork == actWork, 
                    $"[{testCase.No}] 労働時間不一致: 期待値={testCase.ExpectedWork}, 実際={actWork}");
                Assert.True(testCase.ExpectedOvertime == actOvertime, 
                    $"[{testCase.No}] 残業時間不一致: 期待値={testCase.ExpectedOvertime}, 実際={actOvertime}");
                Assert.True(testCase.ExpectedMidnight == actMidnight, 
                    $"[{testCase.No}] 深夜時間不一致: 期待値={testCase.ExpectedMidnight}, 実際={actMidnight}");
                Assert.True(testCase.ExpectedHolidayWork == actHoliday, 
                    $"[{testCase.No}] 休日労働不一致: 期待値={testCase.ExpectedHolidayWork}, 実際={actHoliday}");
            }
        }

        private DateTime ParseDateTime(string dateStr, string timeStr)
        {
            DateTime baseDate = DateTime.Parse(dateStr);
            bool isNextDay = timeStr.StartsWith("翌");
            timeStr = timeStr.Replace("翌", "").Trim();

            TimeSpan time = TimeSpan.Parse(timeStr);
            DateTime result = baseDate.Add(time);
            return isNextDay ? result.AddDays(1) : result;
        }

        private List<(DateTime Start, DateTime End)> ParseBreakStamps(string dateStr, DateTime clockIn, string breakStampStr)
        {
            var breakList = new List<(DateTime Start, DateTime End)>();

            if (string.IsNullOrEmpty(breakStampStr) || breakStampStr == "なし")
            {
                return breakList;
            }

            string[] rawRanges = breakStampStr.Split(new[] { ',', '、' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var rawRange in rawRanges)
            {
                string range = rawRange.Trim();
                if (range.Contains("〜") || range.Contains("~") || range.Contains("-"))
                {
                    string[] parts = range.Split(new[] { '〜', '~', '-' }, StringSplitOptions.RemoveEmptyEntries);
                    
                    DateTime start = ParseDateTime(dateStr, parts[0].Trim());
                    DateTime end = ParseDateTime(dateStr, parts[1].Trim());

                    if (start < clockIn)
                    {
                        start = start.AddDays(1);
                        end = end.AddDays(1);
                    }
                    else if (end <= start)
                    {
                        end = end.AddDays(1);
                    }

                    breakList.Add((start, end));
                }
            }

            return breakList;
        }
    }
}