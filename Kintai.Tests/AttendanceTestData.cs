using System.Collections.Generic;

namespace Kintai.Tests
{
    public class TestCaseData
    {
        public int No { get; set; }
        public string Date { get; set; }
        public string ClockIn { get; set; }
        public string ClockOut { get; set; }
        public string BreakStamp { get; set; }
        public bool IsHoliday { get; set; }
        
        // 期待値
        public string ExpectedBreak { get; set; }
        public string ExpectedWork { get; set; }
        public string ExpectedOvertime { get; set; }
        public string ExpectedMidnight { get; set; }
        public string ExpectedHolidayWork { get; set; }
    }

    public static class AttendanceTestData
    {
        public static List<TestCaseData> Get17TestCases() => new List<TestCaseData>
        {
            new TestCaseData { No = 1, Date = "2026-10-05", ClockIn = "09:00", ClockOut = "18:00", BreakStamp = "12:00〜13:00", IsHoliday = false, ExpectedBreak = "1:00", ExpectedWork = "8:00", ExpectedOvertime = "0:00", ExpectedMidnight = "0:00", ExpectedHolidayWork = "0:00" },
            new TestCaseData { No = 2, Date = "2026-10-06", ClockIn = "09:00", ClockOut = "15:00", BreakStamp = "なし", IsHoliday = false, ExpectedBreak = "0:00", ExpectedWork = "6:00", ExpectedOvertime = "0:00", ExpectedMidnight = "0:00", ExpectedHolidayWork = "0:00" },
            new TestCaseData { No = 3, Date = "2026-10-07", ClockIn = "09:00", ClockOut = "15:01", BreakStamp = "なし", IsHoliday = false, ExpectedBreak = "0:01", ExpectedWork = "6:00", ExpectedOvertime = "0:00", ExpectedMidnight = "0:00", ExpectedHolidayWork = "0:00" },
            new TestCaseData { No = 4, Date = "2026-10-08", ClockIn = "09:00", ClockOut = "17:00", BreakStamp = "なし", IsHoliday = false, ExpectedBreak = "0:45", ExpectedWork = "7:15", ExpectedOvertime = "0:00", ExpectedMidnight = "0:00", ExpectedHolidayWork = "0:00" },
            new TestCaseData { No = 5, Date = "2026-10-09", ClockIn = "09:00", ClockOut = "17:01", BreakStamp = "なし", IsHoliday = false, ExpectedBreak = "0:45", ExpectedWork = "7:16", ExpectedOvertime = "0:00", ExpectedMidnight = "0:00", ExpectedHolidayWork = "0:00" },
            new TestCaseData { No = 6, Date = "2026-10-09", ClockIn = "09:00", ClockOut = "16:00", BreakStamp = "0:30", IsHoliday = false, ExpectedBreak = "0:45", ExpectedWork = "6:15", ExpectedOvertime = "0:00", ExpectedMidnight = "0:00", ExpectedHolidayWork = "0:00" },
            new TestCaseData { No = 7, Date = "2026-10-09", ClockIn = "09:00", ClockOut = "17:45", BreakStamp = "なし", IsHoliday = false, ExpectedBreak = "0:45", ExpectedWork = "8:00", ExpectedOvertime = "0:00", ExpectedMidnight = "0:00", ExpectedHolidayWork = "0:00" },
            // No.8: 労働時間を 8:01 に更新
            new TestCaseData { No = 8, Date = "2026-10-13", ClockIn = "09:00", ClockOut = "18:01", BreakStamp = "12:00〜13:00", IsHoliday = false, ExpectedBreak = "1:00", ExpectedWork = "8:01", ExpectedOvertime = "0:01", ExpectedMidnight = "0:00", ExpectedHolidayWork = "0:00" },
            new TestCaseData { No = 9, Date = "2026-10-14", ClockIn = "13:00", ClockOut = "22:00", BreakStamp = "17:00〜18:00", IsHoliday = false, ExpectedBreak = "1:00", ExpectedWork = "8:00", ExpectedOvertime = "0:00", ExpectedMidnight = "0:00", ExpectedHolidayWork = "0:00" },
            // No.10: 労働時間を 8:01 に更新
            new TestCaseData { No = 10, Date = "2026-10-15", ClockIn = "13:00", ClockOut = "22:01", BreakStamp = "17:00〜18:00", IsHoliday = false, ExpectedBreak = "1:00", ExpectedWork = "8:01", ExpectedOvertime = "0:01", ExpectedMidnight = "0:01", ExpectedHolidayWork = "0:00" },
            new TestCaseData { No = 11, Date = "2026-10-16", ClockIn = "21:00", ClockOut = "翌05:00", BreakStamp = "01:00〜02:00", IsHoliday = false, ExpectedBreak = "1:00", ExpectedWork = "7:00", ExpectedOvertime = "0:00", ExpectedMidnight = "6:00", ExpectedHolidayWork = "0:00" },
            new TestCaseData { No = 12, Date = "2026-10-19", ClockIn = "21:00", ClockOut = "翌05:01", BreakStamp = "01:00〜02:00", IsHoliday = false, ExpectedBreak = "1:00", ExpectedWork = "7:01", ExpectedOvertime = "0:00", ExpectedMidnight = "6:00", ExpectedHolidayWork = "0:00" },
            // No.13: 労働時間を 16:00 に更新
            new TestCaseData { No = 13, Date = "2026-10-23", ClockIn = "09:00", ClockOut = "翌02:00", BreakStamp = "12:00〜13:00", IsHoliday = false, ExpectedBreak = "1:00", ExpectedWork = "16:00", ExpectedOvertime = "8:00", ExpectedMidnight = "4:00", ExpectedHolidayWork = "0:00" },
            new TestCaseData { No = 14, Date = "2026-10-24", ClockIn = "09:00", ClockOut = "19:00", BreakStamp = "12:00〜13:00", IsHoliday = true, ExpectedBreak = "1:00", ExpectedWork = "9:00", ExpectedOvertime = "0:00", ExpectedMidnight = "0:00", ExpectedHolidayWork = "9:00" },
            new TestCaseData { No = 15, Date = "2026-10-25", ClockIn = "18:00", ClockOut = "翌02:00", BreakStamp = "21:00〜22:00", IsHoliday = true, ExpectedBreak = "1:00", ExpectedWork = "7:00", ExpectedOvertime = "0:00", ExpectedMidnight = "4:00", ExpectedHolidayWork = "7:00" },
            new TestCaseData { No = 16, Date = "2026-10-26", ClockIn = "01:00", ClockOut = "10:00", BreakStamp = "04:00〜05:00", IsHoliday = false, ExpectedBreak = "1:00", ExpectedWork = "8:00", ExpectedOvertime = "0:00", ExpectedMidnight = "3:00", ExpectedHolidayWork = "0:00" },
            new TestCaseData { No = 17, Date = "2026-10-27", ClockIn = "21:00", ClockOut = "翌06:00", BreakStamp = "01:00〜02:00", IsHoliday = false, ExpectedBreak = "1:00", ExpectedWork = "8:00", ExpectedOvertime = "0:00", ExpectedMidnight = "6:00", ExpectedHolidayWork = "0:00" }
        };
    }
}