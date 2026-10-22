using System;

namespace KintaiSystem.Models.Entities
{
    public class TimeStamp
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public DateTime StampedAt { get; set; }
        public string StampType { get; set; } = string.Empty; // 出勤, 退勤, 休憩開始, 休憩終了
        public int DailyAttendanceId { get; set; }

        public DailyAttendance? DailyAttendance { get; set; }
    }
}