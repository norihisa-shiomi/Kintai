using System.Collections.Generic;

namespace Kintai.Tests
{

    public class AcceptanceTestCaseData
    {
        public string No { get; set; }
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

    // クラス名を AcceptanceTestData に設定
    public static class AcceptanceTestData
    {
        public static List<AcceptanceTestCaseData> Get37TestCases() => new List<AcceptanceTestCaseData>
        {
            new AcceptanceTestCaseData { No = "K01", Date = "2026-10-05", ClockIn = "09:00", ClockOut = "18:00", BreakStamp = "12:00〜13:00", IsHoliday = false, ExpectedBreak = "1:00", ExpectedWork = "8:00", ExpectedOvertime = "0:00", ExpectedMidnight = "0:00", ExpectedHolidayWork = "0:00" },
            new AcceptanceTestCaseData { No = "K02", Date = "2026-10-05", ClockIn = "09:00", ClockOut = "12:00", BreakStamp = "なし", IsHoliday = false, ExpectedBreak = "0:00", ExpectedWork = "3:00", ExpectedOvertime = "0:00", ExpectedMidnight = "0:00", ExpectedHolidayWork = "0:00" },
            new AcceptanceTestCaseData { No = "K03", Date = "2026-10-05", ClockIn = "09:00", ClockOut = "15:00", BreakStamp = "なし", IsHoliday = false, ExpectedBreak = "0:00", ExpectedWork = "6:00", ExpectedOvertime = "0:00", ExpectedMidnight = "0:00", ExpectedHolidayWork = "0:00" },
            new AcceptanceTestCaseData { No = "K04", Date = "2026-10-05", ClockIn = "09:00", ClockOut = "15:01", BreakStamp = "なし", IsHoliday = false, ExpectedBreak = "0:01", ExpectedWork = "6:00", ExpectedOvertime = "0:00", ExpectedMidnight = "0:00", ExpectedHolidayWork = "0:00" },
            new AcceptanceTestCaseData { No = "K05", Date = "2026-10-05", ClockIn = "09:00", ClockOut = "15:45", BreakStamp = "なし", IsHoliday = false, ExpectedBreak = "0:45", ExpectedWork = "6:00", ExpectedOvertime = "0:00", ExpectedMidnight = "0:00", ExpectedHolidayWork = "0:00" },
            new AcceptanceTestCaseData { No = "K06", Date = "2026-10-05", ClockIn = "09:00", ClockOut = "15:46", BreakStamp = "なし", IsHoliday = false, ExpectedBreak = "0:45", ExpectedWork = "6:01", ExpectedOvertime = "0:00", ExpectedMidnight = "0:00", ExpectedHolidayWork = "0:00" },
            new AcceptanceTestCaseData { No = "K07", Date = "2026-10-05", ClockIn = "09:00", ClockOut = "15:45", BreakStamp = "12:00〜12:45", IsHoliday = false, ExpectedBreak = "0:45", ExpectedWork = "6:00", ExpectedOvertime = "0:00", ExpectedMidnight = "0:00", ExpectedHolidayWork = "0:00" },
            new AcceptanceTestCaseData { No = "K08", Date = "2026-10-05", ClockIn = "09:00", ClockOut = "16:00", BreakStamp = "12:00〜12:30", IsHoliday = false, ExpectedBreak = "0:45", ExpectedWork = "6:15", ExpectedOvertime = "0:00", ExpectedMidnight = "0:00", ExpectedHolidayWork = "0:00" },
            new AcceptanceTestCaseData { No = "K09", Date = "2026-10-05", ClockIn = "09:00", ClockOut = "17:45", BreakStamp = "なし", IsHoliday = false, ExpectedBreak = "0:45", ExpectedWork = "8:00", ExpectedOvertime = "0:00", ExpectedMidnight = "0:00", ExpectedHolidayWork = "0:00" },
            new AcceptanceTestCaseData { No = "K10", Date = "2026-10-05", ClockIn = "09:00", ClockOut = "17:45", BreakStamp = "12:00〜12:45", IsHoliday = false, ExpectedBreak = "0:45", ExpectedWork = "8:00", ExpectedOvertime = "0:00", ExpectedMidnight = "0:00", ExpectedHolidayWork = "0:00" },
            new AcceptanceTestCaseData { No = "K11", Date = "2026-10-05", ClockIn = "09:00", ClockOut = "17:46", BreakStamp = "12:00〜12:45", IsHoliday = false, ExpectedBreak = "0:46", ExpectedWork = "8:00", ExpectedOvertime = "0:00", ExpectedMidnight = "0:00", ExpectedHolidayWork = "0:00" },
            new AcceptanceTestCaseData { No = "K12", Date = "2026-10-05", ClockIn = "09:00", ClockOut = "18:00", BreakStamp = "なし", IsHoliday = false, ExpectedBreak = "1:00", ExpectedWork = "8:00", ExpectedOvertime = "0:00", ExpectedMidnight = "0:00", ExpectedHolidayWork = "0:00" },
            new AcceptanceTestCaseData { No = "K13", Date = "2026-10-05", ClockIn = "09:00", ClockOut = "18:01", BreakStamp = "12:00〜13:00", IsHoliday = false, ExpectedBreak = "1:00", ExpectedWork = "8:01", ExpectedOvertime = "0:01", ExpectedMidnight = "0:00", ExpectedHolidayWork = "0:00" },
            new AcceptanceTestCaseData { No = "K14", Date = "2026-10-05", ClockIn = "09:00", ClockOut = "19:00", BreakStamp = "12:00〜12:45", IsHoliday = false, ExpectedBreak = "1:00", ExpectedWork = "9:00", ExpectedOvertime = "1:00", ExpectedMidnight = "0:00", ExpectedHolidayWork = "0:00" },
            new AcceptanceTestCaseData { No = "K15", Date = "2026-10-05", ClockIn = "09:00", ClockOut = "19:00", BreakStamp = "12:00〜12:30, 15:00〜15:30", IsHoliday = false, ExpectedBreak = "1:00", ExpectedWork = "9:00", ExpectedOvertime = "1:00", ExpectedMidnight = "0:00", ExpectedHolidayWork = "0:00" },
            new AcceptanceTestCaseData { No = "K16", Date = "2026-10-05", ClockIn = "08:59:59", ClockOut = "18:00:30", BreakStamp = "なし", IsHoliday = false, ExpectedBreak = "1:00", ExpectedWork = "8:01", ExpectedOvertime = "0:01", ExpectedMidnight = "0:00", ExpectedHolidayWork = "0:00" },
            new AcceptanceTestCaseData { No = "K17", Date = "2026-10-05", ClockIn = "09:00", ClockOut = "22:00", BreakStamp = "12:00〜13:00", IsHoliday = false, ExpectedBreak = "1:00", ExpectedWork = "12:00", ExpectedOvertime = "4:00", ExpectedMidnight = "0:00", ExpectedHolidayWork = "0:00" },
            new AcceptanceTestCaseData { No = "K18", Date = "2026-10-05", ClockIn = "09:00", ClockOut = "22:01", BreakStamp = "12:00〜13:00", IsHoliday = false, ExpectedBreak = "1:00", ExpectedWork = "12:01", ExpectedOvertime = "4:01", ExpectedMidnight = "0:01", ExpectedHolidayWork = "0:00" },
            new AcceptanceTestCaseData { No = "K19", Date = "2026-10-05", ClockIn = "09:00", ClockOut = "23:30", BreakStamp = "12:00〜13:00", IsHoliday = false, ExpectedBreak = "1:00", ExpectedWork = "13:30", ExpectedOvertime = "5:30", ExpectedMidnight = "1:30", ExpectedHolidayWork = "0:00" },
            new AcceptanceTestCaseData { No = "K20", Date = "2026-10-05", ClockIn = "13:00", ClockOut = "翌02:00", BreakStamp = "18:00〜19:00", IsHoliday = false, ExpectedBreak = "1:00", ExpectedWork = "12:00", ExpectedOvertime = "4:00", ExpectedMidnight = "4:00", ExpectedHolidayWork = "0:00" },
            new AcceptanceTestCaseData { No = "K21", Date = "2026-10-05", ClockIn = "22:00", ClockOut = "翌07:00", BreakStamp = "翌02:00〜翌03:00", IsHoliday = false, ExpectedBreak = "1:00", ExpectedWork = "8:00", ExpectedOvertime = "0:00", ExpectedMidnight = "6:00", ExpectedHolidayWork = "0:00" },
            new AcceptanceTestCaseData { No = "K22", Date = "2026-10-05", ClockIn = "22:00", ClockOut = "翌07:00", BreakStamp = "なし", IsHoliday = false, ExpectedBreak = "1:00", ExpectedWork = "8:00", ExpectedOvertime = "0:00", ExpectedMidnight = "6:00", ExpectedHolidayWork = "0:00" },
            new AcceptanceTestCaseData { No = "K23", Date = "2026-10-05", ClockIn = "22:00", ClockOut = "翌05:00", BreakStamp = "なし", IsHoliday = false, ExpectedBreak = "0:45", ExpectedWork = "6:15", ExpectedOvertime = "0:00", ExpectedMidnight = "6:15", ExpectedHolidayWork = "0:00" },
            new AcceptanceTestCaseData { No = "K24", Date = "2026-10-05", ClockIn = "18:00", ClockOut = "翌03:00", BreakStamp = "23:00〜23:30", IsHoliday = false, ExpectedBreak = "1:00", ExpectedWork = "8:00", ExpectedOvertime = "0:00", ExpectedMidnight = "4:00", ExpectedHolidayWork = "0:00" },
            new AcceptanceTestCaseData { No = "K25", Date = "2026-10-05", ClockIn = "04:00", ClockOut = "13:00", BreakStamp = "10:00〜11:00", IsHoliday = false, ExpectedBreak = "1:00", ExpectedWork = "8:00", ExpectedOvertime = "0:00", ExpectedMidnight = "1:00", ExpectedHolidayWork = "0:00" },
            new AcceptanceTestCaseData { No = "K26", Date = "2026-10-05", ClockIn = "05:00", ClockOut = "14:00", BreakStamp = "11:00〜12:00", IsHoliday = false, ExpectedBreak = "1:00", ExpectedWork = "8:00", ExpectedOvertime = "0:00", ExpectedMidnight = "0:00", ExpectedHolidayWork = "0:00" },
            new AcceptanceTestCaseData { No = "K27", Date = "2026-10-05", ClockIn = "04:59", ClockOut = "14:00", BreakStamp = "11:00〜12:00", IsHoliday = false, ExpectedBreak = "1:00", ExpectedWork = "8:01", ExpectedOvertime = "0:01", ExpectedMidnight = "0:01", ExpectedHolidayWork = "0:00" },
            new AcceptanceTestCaseData { No = "K28", Date = "2026-10-05", ClockIn = "09:00", ClockOut = "翌08:59", BreakStamp = "12:00〜13:00, 18:00〜19:00", IsHoliday = false, ExpectedBreak = "2:00", ExpectedWork = "21:59", ExpectedOvertime = "13:59", ExpectedMidnight = "7:00", ExpectedHolidayWork = "0:00" },
            new AcceptanceTestCaseData { No = "K29", Date = "2026-10-10", ClockIn = "09:00", ClockOut = "18:00", BreakStamp = "12:00〜13:00", IsHoliday = true,  ExpectedBreak = "1:00", ExpectedWork = "8:00", ExpectedOvertime = "0:00", ExpectedMidnight = "0:00", ExpectedHolidayWork = "8:00" },
            new AcceptanceTestCaseData { No = "K30", Date = "2026-10-11", ClockIn = "09:00", ClockOut = "20:00", BreakStamp = "12:00〜13:00", IsHoliday = true,  ExpectedBreak = "1:00", ExpectedWork = "10:00", ExpectedOvertime = "0:00", ExpectedMidnight = "0:00", ExpectedHolidayWork = "10:00" },
            new AcceptanceTestCaseData { No = "K31", Date = "2026-10-12", ClockIn = "09:00", ClockOut = "18:00", BreakStamp = "12:00〜13:00", IsHoliday = true,  ExpectedBreak = "1:00", ExpectedWork = "8:00", ExpectedOvertime = "0:00", ExpectedMidnight = "0:00", ExpectedHolidayWork = "8:00" },
            new AcceptanceTestCaseData { No = "K32", Date = "2026-10-10", ClockIn = "20:00", ClockOut = "翌02:00", BreakStamp = "なし", IsHoliday = true,  ExpectedBreak = "0:00", ExpectedWork = "6:00", ExpectedOvertime = "0:00", ExpectedMidnight = "4:00", ExpectedHolidayWork = "6:00" },
            new AcceptanceTestCaseData { No = "K33", Date = "2026-10-09", ClockIn = "20:00", ClockOut = "翌06:00", BreakStamp = "翌00:00〜翌01:00", IsHoliday = false, ExpectedBreak = "1:00", ExpectedWork = "9:00", ExpectedOvertime = "1:00", ExpectedMidnight = "6:00", ExpectedHolidayWork = "0:00" },
            new AcceptanceTestCaseData { No = "K34", Date = "2026-10-31", ClockIn = "22:00", ClockOut = "翌06:00", BreakStamp = "翌01:00〜翌02:00", IsHoliday = true,  ExpectedBreak = "1:00", ExpectedWork = "7:00", ExpectedOvertime = "0:00", ExpectedMidnight = "6:00", ExpectedHolidayWork = "7:00" },
            new AcceptanceTestCaseData { No = "K35", Date = "2026-12-29", ClockIn = "09:00", ClockOut = "18:00", BreakStamp = "12:00〜13:00", IsHoliday = true,  ExpectedBreak = "1:00", ExpectedWork = "8:00", ExpectedOvertime = "0:00", ExpectedMidnight = "0:00", ExpectedHolidayWork = "8:00" },
            new AcceptanceTestCaseData { No = "K36", Date = "2026-10-06", ClockIn = "21:00", ClockOut = "翌06:00", BreakStamp = "なし", IsHoliday = false, ExpectedBreak = "1:00", ExpectedWork = "8:00", ExpectedOvertime = "0:00", ExpectedMidnight = "6:00", ExpectedHolidayWork = "0:00" },
            new AcceptanceTestCaseData { No = "K37", Date = "2026-10-05", ClockIn = "09:00", ClockOut = "15:30", BreakStamp = "なし", IsHoliday = false, ExpectedBreak = "0:30", ExpectedWork = "6:00", ExpectedOvertime = "0:00", ExpectedMidnight = "0:00", ExpectedHolidayWork = "0:00" }
        };
    }
}