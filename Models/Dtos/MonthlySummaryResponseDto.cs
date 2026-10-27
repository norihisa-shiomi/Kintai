using System.Collections.Generic;

namespace Kintai.Models.Dtos
{
    /// <summary>
    /// SC-09 月次集計画面 レスポンスDTO
    /// </summary>
    public class MonthlySummaryResponseDto
    {
        public string TargetYearMonth { get; set; } = string.Empty; // "2026-10"
        public string LockStatus { get; set; } = "未締め（編集可）";
        public bool IsClosed { get; set; } = false;

        // 全体サマリー (SC-09 No.4)
        public int TotalEmployeeCount { get; set; }
        public string OverallTotalWorkHours { get; set; } = "0:00";
        public string OverallTotalOvertimeHours { get; set; } = "0:00";

        // 全社員集計行リスト (SC-09 No.5)
        public List<EmployeeMonthlySummaryRowDto> EmployeeSummaries { get; set; } = new();
    }

    public class EmployeeMonthlySummaryRowDto
    {
        public int EmployeeId { get; set; }
        public string EmployeeNumber { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        
        public int WorkDays { get; set; }
        public int HolidayWorkDays { get; set; }
        
        public string TotalWorkHours { get; set; } = "0:00";
        public string TotalOvertimeHours { get; set; } = "0:00";
        public string TotalMidnightHours { get; set; } = "0:00";
        public string TotalHolidayWorkHours { get; set; } = "0:00";

        // 法定休憩自動補正警告 (SC-09 No.6)
        public bool HasBreakAdjustmentWarning { get; set; }
    }
}