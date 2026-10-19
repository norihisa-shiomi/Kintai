using System;

namespace KintaiSystem.Models
{
    /// <summary>
    /// CSV出力用データ転送オブジェクト（1社員の月間集計情報）
    /// </summary>
    public class MonthlySummaryCsvDto
    {
        public string EmployeeNumber { get; set; } = string.Empty; // 社員番号 (例: E0001)
        public string Name { get; set; } = string.Empty;           // 氏名
        public string TargetYearMonth { get; set; } = string.Empty;// 対象年月 (例: 2026-10)
        public int WorkDays { get; set; }                          // 出勤日数
        public int HolidayWorkDays { get; set; }                   // 休日出勤日数
        public int TotalWorkMinutes { get; set; }                 // 総労働時間（分単位）
        public int TotalOvertimeMinutes { get; set; }             // 総残業時間（分単位）
        public int TotalMidnightMinutes { get; set; }             // 総深夜時間（分単位）
        public int TotalHolidayWorkMinutes { get; set; }         // 総休日労働時間（分単位）
    }
}
