using System;
using System.Collections.Generic;
using System.Linq;

namespace Kintai.Services
{
    public class AttendanceResult
    {
        public TimeSpan BreakTime { get; set; }
        public TimeSpan WorkHours { get; set; }
        public TimeSpan OvertimeHours { get; set; }
        public TimeSpan MidnightHours { get; set; }
        public TimeSpan HolidayWorkHours { get; set; }

        /// <summary>
        /// 法定休憩の自動補正が行われたか（SC-09 補正警告アイコン用）
        /// </summary>
        public bool HasBreakAdjustment { get; set; }

        public string FormatTime(TimeSpan time) => $"{(int)time.TotalHours}:{time.Minutes:D2}";
    }

    public static class AttendanceCalculator
    {
        /// <summary>
        /// 単一休憩（SC-05 月次一覧等の既存呼び出し）に対応するオーバーロード
        /// </summary>
        public static AttendanceResult Calculate(
            DateTime clockIn, 
            DateTime clockOut, 
            (DateTime Start, DateTime End)? breakStamp, 
            bool isHoliday)
        {
            var breakList = new List<(DateTime Start, DateTime End)>();
            if (breakStamp.HasValue)
            {
                breakList.Add((breakStamp.Value.Start, breakStamp.Value.End));
            }

            return Calculate(clockIn, clockOut, breakList, isHoliday);
        }

        /// <summary>
        /// 1日の勤怠時間（拘束・休憩補正・実働・残業・深夜・休日）を計算する（複数休憩対応）
        /// </summary>
        public static AttendanceResult Calculate(
            DateTime clockIn, 
            DateTime clockOut, 
            IEnumerable<(DateTime Start, DateTime End)> breakStamps, 
            bool isHoliday)
        {
            var result = new AttendanceResult();
            var breaks = breakStamps?.ToList() ?? new List<(DateTime Start, DateTime End)>();

            // 1. 拘束時間（秒切捨て・1分単位）
            var cleanIn = new DateTime(clockIn.Year, clockIn.Month, clockIn.Day, clockIn.Hour, clockIn.Minute, 0);
            var cleanOut = new DateTime(clockOut.Year, clockOut.Month, clockOut.Day, clockOut.Hour, clockOut.Minute, 0);
            var boundMinutes = (int)(cleanOut - cleanIn).TotalMinutes;
            if (boundMinutes < 0) boundMinutes = 0;

            // 2. 実打刻休憩合計（複数回の休憩に対応）
            int actualBreakMinutes = 0;
            foreach (var b in breaks)
            {
                var bStart = new DateTime(b.Start.Year, b.Start.Month, b.Start.Day, b.Start.Hour, b.Start.Minute, 0);
                var bEnd = new DateTime(b.End.Year, b.End.Month, b.End.Day, b.End.Hour, b.End.Minute, 0);
                var diff = (int)(bEnd - bStart).TotalMinutes;
                if (diff > 0) actualBreakMinutes += diff;
            }

            // 3. 法定必要休憩分数の判定（外部設計書 6-1 定義）
            int requiredBreakMinutes = GetRequiredBreakMinutes(boundMinutes);

            // 法定枠に満たない場合、不足分を自動補正（差分控除）し警告フラグをONにする
            int effectiveBreakMinutes = actualBreakMinutes;
            if (actualBreakMinutes < requiredBreakMinutes)
            {
                effectiveBreakMinutes = requiredBreakMinutes;
                result.HasBreakAdjustment = true;
            }
            result.BreakTime = TimeSpan.FromMinutes(effectiveBreakMinutes);

            // 実労働時間 = 拘束時間 - 休憩時間
            int netWorkMinutes = Math.Max(0, boundMinutes - effectiveBreakMinutes);
            result.WorkHours = TimeSpan.FromMinutes(netWorkMinutes);

            // 4. 深夜時間の算出 (22:00 ～ 05:00 の実労働時間・休憩時間は除外)
            result.MidnightHours = CalculateMidnightHours(cleanIn, cleanOut, breaks);

            // 5. 休日 vs 平日・残業判定
            if (isHoliday)
            {
                // 休日はすべて休日労働時間（8時間を超えても残業は0）
                result.HolidayWorkHours = result.WorkHours;
                result.OvertimeHours = TimeSpan.Zero;
            }
            else
            {
                result.HolidayWorkHours = TimeSpan.Zero;
                // 平日8時間（480分）を超えた分を残業時間とする
                if (netWorkMinutes > 480)
                {
                    result.OvertimeHours = TimeSpan.FromMinutes(netWorkMinutes - 480);
                }
                else
                {
                    result.OvertimeHours = TimeSpan.Zero;
                }
            }

            return result;
        }

        /// <summary>
        /// 拘束時間に応じた法定最低休憩分数の算出（外部設計書 6-1 仕様）
        /// </summary>
        private static int GetRequiredBreakMinutes(int boundMinutes)
        {
            if (boundMinutes <= 360) return 0;                    // 6時間00分以下: 0分
            if (boundMinutes <= 404) return boundMinutes - 360;   // 6時間01分〜6時間44分: 超過分数のみ
            if (boundMinutes <= 525) return 45;                   // 6時間45分〜8時間45分: 45分
            if (boundMinutes < 540) return boundMinutes - 480;    // 8時間46分〜8時間59分: 超過分数のみ
            return 60;                                            // 9時間以上: 60分
        }

        /// <summary>
        /// 22:00〜翌5:00の時間帯から休憩時間を除いた実働分数を算出
        /// </summary>
        private static TimeSpan CalculateMidnightHours(
            DateTime cleanIn, 
            DateTime cleanOut, 
            List<(DateTime Start, DateTime End)> breaks)
        {
            int midnightMinutes = 0;
            DateTime current = cleanIn;

            while (current < cleanOut)
            {
                int hour = current.Hour;
                // 22時以降または翌5時前
                if (hour >= 22 || hour < 5)
                {
                    // 打刻休憩時間帯に含まれていないか判定
                    bool isInBreak = breaks.Any(b => current >= b.Start && current < b.End);
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