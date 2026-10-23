using System;

namespace KintaiSystem.Models.Entities
{
    public class BreakTime
    {
        public int Id { get; set; }
        public int DailyAttendanceId { get; set; }
        public DateTime BreakStartTime { get; set; }
        public DateTime? BreakEndTime { get; set; }

        public DailyAttendance? DailyAttendance { get; set; }
    }
}