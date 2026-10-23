using System;
using System.Collections.Generic;

namespace KintaiSystem.Models.Entities
{
    public class DailyAttendance
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public DateTime WorkDate { get; set; }
        public string DayOfWeek { get; set; } = string.Empty;
        public string CurrentStatus { get; set; } = "勤務前"; // 勤務前, 勤務中, 休憩中, 退勤済み
        public DateTime? ClockInTime { get; set; }
        public DateTime? ClockOutTime { get; set; }

        public ICollection<BreakTime> BreakTimes { get; set; } = new List<BreakTime>();
        public ICollection<TimeStamp> TimeStamps { get; set; } = new List<TimeStamp>();
    }
}