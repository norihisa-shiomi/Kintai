using System;

namespace Kintai.Services
{
    public class AttendanceResult
    {
        public TimeSpan BreakTime { get; set; }
        public TimeSpan WorkHours { get; set; }
        public TimeSpan OvertimeHours { get; set; }
        public TimeSpan MidnightHours { get; set; }
        public TimeSpan HolidayWorkHours { get; set; }

        public string FormatTime(TimeSpan time) => $"{(int)time.TotalHours}:{time.Minutes:D2}";
    }

    public class AttendanceCalculator
    {
        public static AttendanceResult Calculate(
            DateTime clockIn, 
            DateTime clockOut, 
            (DateTime Start, DateTime End)? breakStamp, 
            bool isHoliday)
        {
            var result = new AttendanceResult();

            // 1. 拘束時間（1分単位・秒切捨て）
            TimeSpan boundTime = TimeSpan.FromMinutes((int)(clockOut - clockIn).TotalMinutes);

            // 2. 実打刻休憩時間
            TimeSpan actualBreak = TimeSpan.Zero;
            if (breakStamp.HasValue)
            {
                actualBreak = TimeSpan.FromMinutes((int)(breakStamp.Value.End - breakStamp.Value.Start).TotalMinutes);
            }

            // 3. 法定休憩自動補正（6-1定義）
            TimeSpan effectiveBreak = actualBreak;
            if (actualBreak.TotalMinutes < GetRequiredBreakMinutes(boundTime))
            {
                effectiveBreak = TimeSpan.FromMinutes(GetRequiredBreakMinutes(boundTime));
            }

            result.BreakTime = effectiveBreak;

            // 実労働時間 = 拘束時間 - 休憩時間
            TimeSpan netWorkTime = boundTime - effectiveBreak;
            if (netWorkTime < TimeSpan.Zero) netWorkTime = TimeSpan.Zero;

            // 4. 深夜時間の算出 (22:00 ～ 05:00)
            TimeSpan midnight = CalculateMidnightHours(clockIn, clockOut, breakStamp);
            result.MidnightHours = midnight;

            // 5. 休日労働 vs 平日労働・残業
            if (isHoliday)
            {
                result.WorkHours = netWorkTime;         // ★休日でも実労働時間としてセット（例: 9:00）
                result.HolidayWorkHours = netWorkTime;  // 休日労働時間にもセット（例: 9:00）
                result.OvertimeHours = TimeSpan.Zero;   // 休日は残業なし（すべて休日労働に集約）
            }
            else
            {
                result.HolidayWorkHours = TimeSpan.Zero;
                if (netWorkTime.TotalHours > 8)
                {
                    result.WorkHours = TimeSpan.FromHours(8);
                    result.OvertimeHours = netWorkTime - TimeSpan.FromHours(8);
                }
                else
                {
                    result.WorkHours = netWorkTime;
                    result.OvertimeHours = TimeSpan.Zero;
                }
            }
            return result;
        }

        private static double GetRequiredBreakMinutes(TimeSpan bound)
        {
            double boundMinutes = bound.TotalMinutes;

            if (boundMinutes <= 360) return 0;
            if (boundMinutes <= 404) return boundMinutes - 360;
            if (boundMinutes <= 525) return 45;
            if (boundMinutes < 540) return boundMinutes - 480;
            return 60;
        }

        private static TimeSpan CalculateMidnightHours(
            DateTime inTime, 
            DateTime outTime, 
            (DateTime Start, DateTime End)? breakStamp)
        {
            double midnightMinutes = 0;
            DateTime current = inTime;

            // 休憩時間が渡されている場合、日付を出勤時間に合わせて調整
            (DateTime Start, DateTime End)? adjustedBreak = null;
            if (breakStamp.HasValue)
            {
                DateTime bStart = breakStamp.Value.Start;
                DateTime bEnd = breakStamp.Value.End;

                // 休憩開始が出勤時刻より前の時刻になっている場合、翌日の時刻として補正
                if (bStart < inTime)
                {
                    bStart = bStart.AddDays(1);
                    bEnd = bEnd.AddDays(1);
                }
                adjustedBreak = (bStart, bEnd);
            }

            while (current < outTime)
            {
                int hour = current.Hour;
                if (hour >= 22 || hour < 5)
                {
                    bool isInBreak = adjustedBreak.HasValue &&
                                     current >= adjustedBreak.Value.Start &&
                                     current < adjustedBreak.Value.End;

                    if (!isInBreak)
                    {
                        midnightMinutes++;
                    }
                }
                current = current.AddMinutes(1);
            }

            return TimeSpan.FromMinutes(midnightMinutes);
        }
    }
}