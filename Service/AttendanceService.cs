using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using KintaiSystem.Data;
using KintaiSystem.Models.Entities;

namespace KintaiSystem.Services
{
    public class AttendanceService
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
            if (attendance != null && attendance.IsClosed)
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
    }
}