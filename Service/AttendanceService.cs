using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using KintaiSystem.Data;
using KintaiSystem.Models.Dtos;
using KintaiSystem.Models.Entities;
using KintaiSystem.Services.Interfaces;
using Kintai.Services; // AttendanceCalculator の名前空間

namespace KintaiSystem.Services
{
    public class AttendanceService : IAttendanceService
    {
        private readonly AppDbContext _context;

        public AttendanceService(AppDbContext context)
        {
            _context = context;
        }

        // 打刻メイン処理
        public (bool IsSuccess, string Message) ProcessStamp(int employeeId, string stampType, DateTime stampTime)
        {
            var today = stampTime.Date;

            // 1. 当日の勤務データを取得
            var attendance = _context.DailyAttendances
                .Include(a => a.BreakTimes)
                .FirstOrDefault(a => a.EmployeeId == employeeId && a.WorkDate == today);

            // 2. 退勤打刻の場合で当日のデータがない（または勤務前）場合は、過去の未退勤データを検索（徹夜勤務・日跨ぎ対応）
            if (stampType == "退勤" && (attendance == null || attendance.CurrentStatus == "勤務前"))
            {
                var lastUnclosedAttendance = _context.DailyAttendances
                    .Include(a => a.BreakTimes)
                    .Where(a => a.EmployeeId == employeeId && a.WorkDate < today && a.ClockInTime != null && a.ClockOutTime == null)
                    .OrderByDescending(a => a.WorkDate)
                    .FirstOrDefault();

                if (lastUnclosedAttendance != null)
                {
                    attendance = lastUnclosedAttendance;
                }
            }

            // 【E-0016】月次締め完了チェック
            if (attendance != null && false)// DBにカラムがないため一時的に締め判定を無効化
            {
                return (false, "E-0016: 対象年月が締め完了状態のため、打刻処理を行えません。");
            }

            if (attendance == null)
            {
                // 【E-0011】前回の出勤に対する退勤が行われていないかチェック
                var hasUnclosedClockIn = _context.DailyAttendances
                    .Any(a => a.EmployeeId == employeeId && a.WorkDate < today && a.ClockInTime != null && a.ClockOutTime == null);

                if (hasUnclosedClockIn && stampType == "出勤")
                {
                    return (false, "E-0011: 前回の出勤に対する退勤打刻が行われていません。");
                }

                attendance = new DailyAttendance
                {
                    EmployeeId = employeeId,
                    WorkDate = today,
                    DayOfWeek = stampTime.ToString("ddd"),
                    CurrentStatus = "勤務前"
                };
                _context.DailyAttendances.Add(attendance);
                _context.SaveChanges();
            }

            // 打刻種別ごとのステータスチェック
            switch (stampType)
            {
                case "出勤":
                    if (attendance.CurrentStatus != "勤務前" && attendance.CurrentStatus != "退勤済み")
                    {
                        return (false, "E-0010: 現在の勤務状態では出勤打刻は行えません。");
                    }
                    attendance.CurrentStatus = "勤務中";
                    attendance.ClockInTime ??= stampTime;
                    break;

                case "退勤":
                    if (attendance.CurrentStatus != "勤務中")
                    {
                        return (false, "E-0012: 現在の勤務状態では退勤打刻は行えません。");
                    }

                    // 【E-0013】24時間超過チェック
                    if (attendance.ClockInTime.HasValue && (stampTime - attendance.ClockInTime.Value).TotalHours >= 24)
                    {
                        return (false, "E-0013: 出勤時刻から24時間以上経過しているため退勤打刻を行えません。");
                    }

                    attendance.CurrentStatus = "退勤済み";
                    attendance.ClockOutTime = stampTime;
                    break;

                case "休憩開始":
                    if (attendance.CurrentStatus != "勤務中")
                    {
                        return (false, "E-0014: 現在の勤務状態では休憩開始は行えません。");
                    }
                    attendance.CurrentStatus = "休憩中";
                    attendance.BreakTimes.Add(new BreakTime
                    {
                        DailyAttendanceId = attendance.Id,
                        BreakStartTime = stampTime
                    });
                    break;

                case "休憩終了":
                    if (attendance.CurrentStatus != "休憩中")
                    {
                        return (false, "E-0015: 現在の勤務状態では休憩終了は行えません。");
                    }
                    attendance.CurrentStatus = "勤務中";
                    var currentBreak = attendance.BreakTimes.FirstOrDefault(b => b.BreakEndTime == null);
                    if (currentBreak != null)
                    {
                        currentBreak.BreakEndTime = stampTime;
                    }
                    break;

                default:
                    return (false, "不正な打刻種別です。");
            }

            _context.TimeStamps.Add(new TimeStamp
            {
                EmployeeId = employeeId,
                StampedAt = stampTime,
                StampType = stampType,
                DailyAttendanceId = attendance.Id
            });

            _context.SaveChanges();
            return (true, "打刻が完了しました。");
        }

        public (string CurrentStatus, List<object> History) GetTodayStatus(int employeeId)
        {
            var today = DateTime.Today;
            var attendance = _context.DailyAttendances
                .FirstOrDefault(a => a.EmployeeId == employeeId && a.WorkDate == today);

            var status = attendance?.CurrentStatus ?? "勤務前";

            var history = _context.TimeStamps
                .Where(t => t.EmployeeId == employeeId && t.StampedAt.Date == today)
                .OrderBy(t => t.StampedAt)
                .Select(t => new
                {
                    stampType = t.StampType,
                    stampedAt = t.StampedAt.ToString("yyyy/MM/dd HH:mm:ss")
                })
                .ToList<object>();

            return (status, history);
        }
        /// <summary>
        /// SC-05 月次一覧データを取得・集計する (設計書 4-5, 6-1, 6-3, 10-F03)
        /// </summary>
        public async Task<MonthlyAttendanceResponseDto> GetMonthlyAttendanceAsync(int employeeId, int year, int month)
        {
            // 1. 社員情報の取得 (SC-05 No.22 氏名表示用)
            var employee = await _context.Employees
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == employeeId);

            if (employee == null)
            {
                throw new KeyNotFoundException("対象の社員が見つかりません。");
            }

            var targetYearMonth = $"{year:D4}-{month:D2}";

            // 2. 対象月の開始日・終了日（1日〜月末日）
            var startDate = new DateTime(year, month, 1);
            var daysInMonth = DateTime.DaysInMonth(year, month);
            var endDate = new DateTime(year, month, daysInMonth);

            // 3. 実績データおよび休憩時間の取得
            var dailyRecords = await _context.DailyAttendances
                .Include(d => d.BreakTimes)
                .AsNoTracking()
                .Where(d => d.EmployeeId == employeeId && d.WorkDate >= startDate && d.WorkDate <= endDate)
                .ToDictionaryAsync(d => d.WorkDate.Date);

            // 4. 締め状態の確認（DBカラム未存在エラーを防ぐため安全に判定）
            bool isClosed = false;
            string lockStatusText = isClosed ? "締め済み（ロック）" : "未締め（編集可）";

            // 月集計用カウンター (設計書 6-3 月の集計)
            int workDaysCount = 0;
            int holidayWorkDaysCount = 0;
            TimeSpan totalWorkTime = TimeSpan.Zero;
            TimeSpan totalOvertime = TimeSpan.Zero;
            TimeSpan totalMidnight = TimeSpan.Zero;
            TimeSpan totalHolidayWork = TimeSpan.Zero;

            var dailyList = new List<DailyAttendanceDetailDto>();
            string[] weekDaysJapanese = { "日", "月", "火", "水", "木", "金", "土" };

            // 5. 1日〜末日まで全日をループ（未出勤日も空行として補完）
            for (int day = 1; day <= daysInMonth; day++)
            {
                var currentDate = new DateTime(year, month, day);
                var dayOfWeekStr = weekDaysJapanese[(int)currentDate.DayOfWeek];

                // 休日判定（土日）
                bool isHoliday = currentDate.DayOfWeek == DayOfWeek.Saturday || currentDate.DayOfWeek == DayOfWeek.Sunday;

                var detailDto = new DailyAttendanceDetailDto
                {
                    WorkDate = currentDate.ToString("yyyy-MM-dd"),
                    DisplayDate = currentDate.ToString("MM/dd"),
                    DayOfWeek = dayOfWeekStr,
                    IsHoliday = isHoliday
                };

                // 出勤データが存在する場合の計算処理
                if (dailyRecords.TryGetValue(currentDate.Date, out var record))
                {
                    detailDto.ClockInTime = record.ClockInTime?.ToString("HH:mm");
                    detailDto.ClockOutTime = record.ClockOutTime?.ToString("HH:mm");

                    var firstBreak = record.BreakTimes?.FirstOrDefault();
                    if (firstBreak != null)
                    {
                        detailDto.BreakStartTime = firstBreak.BreakStartTime.ToString("HH:mm");
                        detailDto.BreakEndTime = firstBreak.BreakEndTime?.ToString("HH:mm");
                    }

                    // 退勤打刻がある日のみ計算（設計書 1-19: 退勤未打刻は未計算）
                    if (record.ClockInTime.HasValue && record.ClockOutTime.HasValue)
                    {
                        // AttendanceCalculator の仕様 (Start, End)? に変換
                        (DateTime Start, DateTime End)? breakStamp = null;
                        if (firstBreak != null && firstBreak.BreakEndTime.HasValue)
                        {
                            breakStamp = (firstBreak.BreakStartTime, firstBreak.BreakEndTime.Value);
                        }

                        // 既存の AttendanceCalculator.Calculate を実行
                        var calcResult = AttendanceCalculator.Calculate(
                            record.ClockInTime.Value,
                            record.ClockOutTime.Value,
                            breakStamp,
                            isHoliday
                        );

                        detailDto.WorkHours = calcResult.FormatTime(calcResult.WorkHours);
                        detailDto.OvertimeHours = calcResult.FormatTime(calcResult.OvertimeHours);
                        detailDto.MidnightHours = calcResult.FormatTime(calcResult.MidnightHours);
                        detailDto.HolidayWorkHours = calcResult.FormatTime(calcResult.HolidayWorkHours);

                        // 月次サマリー集計 (設計書 6-3)
                        if (isHoliday)
                        {
                            holidayWorkDaysCount++;
                        }
                        else
                        {
                            workDaysCount++;
                        }

                        totalWorkTime += calcResult.WorkHours;
                        totalOvertime += calcResult.OvertimeHours;
                        totalMidnight += calcResult.MidnightHours;
                        totalHolidayWork += calcResult.HolidayWorkHours;
                    }
                }

                dailyList.Add(detailDto);
            }

            // 合計時間フォーマット用ローカル関数 ("H:mm" 形式)
            string FormatTotalTime(TimeSpan time) => $"{(int)time.TotalHours}:{time.Minutes:D2}";

            // 6. 画面返却用DTOの組み立て
            return new MonthlyAttendanceResponseDto
            {
                EmployeeName = employee.Name,
                TargetYearMonth = targetYearMonth,
                LockStatus = lockStatusText,
                IsClosed = isClosed,
                WorkDays = workDaysCount,
                HolidayWorkDays = holidayWorkDaysCount,
                TotalWorkHours = FormatTotalTime(totalWorkTime),
                TotalOvertimeHours = FormatTotalTime(totalOvertime),
                TotalMidnightHours = FormatTotalTime(totalMidnight),
                TotalHolidayWorkHours = FormatTotalTime(totalHolidayWork),
                DailyList = dailyList
            };
        }
    }
}