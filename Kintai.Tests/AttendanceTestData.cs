using System.Collections.Generic;

namespace Kintai.Tests
{
    public class AttendanceTestCase
    {
        public int No { get; set; }
        public string ClockIn { get; set; } = "";
        public string ClockOut { get; set; } = "";
        public string BreakStamp { get; set; } = "";
        public bool IsHoliday { get; set; }
        public string ExpectedBreak { get; set; } = "";
        public string ExpectedWork { get; set; } = "";
        public string ExpectedOvertime { get; set; } = "";
        public string ExpectedMidnight { get; set; } = "";
        public string ExpectedHolidayWork { get; set; } = "";
    }

    public static class AttendanceTestData
    {
        public static List<AttendanceTestCase> Get17TestCases()
        {
            return new List<AttendanceTestCase>
            {
                // No.1: 標準の定時勤務
                new AttendanceTestCase { No = 1, ClockIn = "2026-10-05 09:00", ClockOut = "2026-10-05 18:00", BreakStamp = "12:00-13:00", IsHoliday = false, ExpectedBreak = "1:00", ExpectedWork = "8:00", ExpectedOvertime = "0:00", ExpectedMidnight = "0:00", ExpectedHolidayWork = "0:00" },
                // No.2: 休憩時間の境界値（拘束6時間）
                new AttendanceTestCase { No = 2, ClockIn = "2026-10-06 09:00", ClockOut = "2026-10-06 15:00", BreakStamp = "", IsHoliday = false, ExpectedBreak = "0:00", ExpectedWork = "6:00", ExpectedOvertime = "0:00", ExpectedMidnight = "0:00", ExpectedHolidayWork = "0:00" },
                // No.3: 休憩時間の境界値（拘束6時間1分）
                new AttendanceTestCase { No = 3, ClockIn = "2026-10-07 09:00", ClockOut = "2026-10-07 15:01", BreakStamp = "", IsHoliday = false, ExpectedBreak = "0:01", ExpectedWork = "6:00", ExpectedOvertime = "0:00", ExpectedMidnight = "0:00", ExpectedHolidayWork = "0:00" },
                // No.4: 休憩時間の境界値（拘束8時間）
                new AttendanceTestCase { No = 4, ClockIn = "2026-10-08 09:00", ClockOut = "2026-10-08 17:00", BreakStamp = "", IsHoliday = false, ExpectedBreak = "0:45", ExpectedWork = "7:15", ExpectedOvertime = "0:00", ExpectedMidnight = "0:00", ExpectedHolidayWork = "0:00" },
                // No.5: 境界値（拘束8時間1分）
                new AttendanceTestCase { No = 5, ClockIn = "2026-10-09 09:00", ClockOut = "2026-10-09 17:01", BreakStamp = "", IsHoliday = false, ExpectedBreak = "0:45", ExpectedWork = "7:16", ExpectedOvertime = "0:00", ExpectedMidnight = "0:00", ExpectedHolidayWork = "0:00" },
                // No.6: 追加テスト（打刻休憩30分＋不足15分自動補填）
                new AttendanceTestCase { No = 6, ClockIn = "2026-10-12 09:00", ClockOut = "2026-10-12 16:00", BreakStamp = "0:30", IsHoliday = false, ExpectedBreak = "0:45", ExpectedWork = "6:15", ExpectedOvertime = "0:00", ExpectedMidnight = "0:00", ExpectedHolidayWork = "0:00" },
                // No.7: 残業時間の境界値（平日実労働8時間）
                new AttendanceTestCase { No = 7, ClockIn = "2026-10-09 09:00", ClockOut = "2026-10-09 17:45", BreakStamp = "", IsHoliday = false, ExpectedBreak = "0:45", ExpectedWork = "8:00", ExpectedOvertime = "0:00", ExpectedMidnight = "0:00", ExpectedHolidayWork = "0:00" },
                // No.8: 残業時間の境界値（平日実労働8時間1分）
                new AttendanceTestCase { No = 8, ClockIn = "2026-10-13 09:00", ClockOut = "2026-10-13 18:01", BreakStamp = "12:00-13:00", IsHoliday = false, ExpectedBreak = "1:00", ExpectedWork = "8:00", ExpectedOvertime = "0:01", ExpectedMidnight = "0:00", ExpectedHolidayWork = "0:00" },
                // No.9: 深夜時間の境界値（22:00退勤）
                new AttendanceTestCase { No = 9, ClockIn = "2026-10-14 13:00", ClockOut = "2026-10-14 22:00", BreakStamp = "17:00-18:00", IsHoliday = false, ExpectedBreak = "1:00", ExpectedWork = "8:00", ExpectedOvertime = "0:00", ExpectedMidnight = "0:00", ExpectedHolidayWork = "0:00" },
                // No.10: 深夜時間の境界値（22:01退勤）
                new AttendanceTestCase { No = 10, ClockIn = "2026-10-15 13:00", ClockOut = "2026-10-15 22:01", BreakStamp = "17:00-18:00", IsHoliday = false, ExpectedBreak = "1:00", ExpectedWork = "8:00", ExpectedOvertime = "0:01", ExpectedMidnight = "0:01", ExpectedHolidayWork = "0:00" },
                // No.11: 深夜時間の境界値（翌05:00退勤）
                new AttendanceTestCase { No = 11, ClockIn = "2026-10-16 21:00", ClockOut = "2026-10-17 05:00", BreakStamp = "01:00-02:00", IsHoliday = false, ExpectedBreak = "1:00", ExpectedWork = "7:00", ExpectedOvertime = "0:00", ExpectedMidnight = "6:00", ExpectedHolidayWork = "0:00" },
                // No.12: 深夜時間の境界値（翌05:01退勤）
                new AttendanceTestCase { No = 12, ClockIn = "2026-10-19 21:00", ClockOut = "2026-10-20 05:01", BreakStamp = "01:00-02:00", IsHoliday = false, ExpectedBreak = "1:00", ExpectedWork = "7:01", ExpectedOvertime = "0:00", ExpectedMidnight = "6:00", ExpectedHolidayWork = "0:00" },
                // No.13: 日またぎ勤務（金曜出勤～土曜未明）
                new AttendanceTestCase { No = 13, ClockIn = "2026-10-23 09:00", ClockOut = "2026-10-24 02:00", BreakStamp = "12:00-13:00", IsHoliday = false, ExpectedBreak = "1:00", ExpectedWork = "8:00", ExpectedOvertime = "8:00", ExpectedMidnight = "4:00", ExpectedHolidayWork = "0:00" },
                // No.14: 休日労働
                new AttendanceTestCase { No = 14, ClockIn = "2026-10-24 09:00", ClockOut = "2026-10-24 19:00", BreakStamp = "12:00-13:00", IsHoliday = true, ExpectedBreak = "1:00", ExpectedWork = "9:00", ExpectedOvertime = "0:00", ExpectedMidnight = "0:00", ExpectedHolidayWork = "9:00" },
                // No.15: 休日労働と深夜の重複
                new AttendanceTestCase { No = 15, ClockIn = "2026-10-25 18:00", ClockOut = "2026-10-26 02:00", BreakStamp = "21:00-22:00", IsHoliday = true, ExpectedBreak = "1:00", ExpectedWork = "7:00", ExpectedOvertime = "0:00", ExpectedMidnight = "4:00", ExpectedHolidayWork = "7:00" },
                // No.16: 1日2回出退勤（1回目）
                new AttendanceTestCase { No = 16, ClockIn = "2026-10-26 01:00", ClockOut = "2026-10-26 10:00", BreakStamp = "04:00-05:00", IsHoliday = false, ExpectedBreak = "1:00", ExpectedWork = "8:00", ExpectedOvertime = "0:00", ExpectedMidnight = "3:00", ExpectedHolidayWork = "0:00" },
                // No.17: 1日2回出退勤（2回目・翌日分扱いの検証）
                new AttendanceTestCase { No = 17, ClockIn = "2026-10-27 21:00", ClockOut = "2026-10-28 06:00", BreakStamp = "01:00-02:00", IsHoliday = false, ExpectedBreak = "1:00", ExpectedWork = "8:00", ExpectedOvertime = "0:00", ExpectedMidnight = "6:00", ExpectedHolidayWork = "0:00" }
            };
        }
    }
}