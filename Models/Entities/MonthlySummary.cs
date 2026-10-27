using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KintaiSystem.Models.Entities
{
    [Table("monthly_summaries")]
    public class MonthlySummary
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("employee_id")]
        public int EmployeeId { get; set; }

        [ForeignKey("EmployeeId")]
        public Employee? Employee { get; set; }

        [Required]
        [Column("target_year_month")]
        [MaxLength(7)]
        public string TargetYearMonth { get; set; } = string.Empty; // "YYYY-MM"

        [Column("work_days")]
        public int WorkDays { get; set; }

        [Column("holiday_work_days")]
        public int HolidayWorkDays { get; set; }

        [Column("total_work_hours")]
        [MaxLength(10)]
        public string TotalWorkHours { get; set; } = "0:00";

        [Column("total_overtime_hours")]
        [MaxLength(10)]
        public string TotalOvertimeHours { get; set; } = "0:00";

        [Column("total_midnight_hours")]
        [MaxLength(10)]
        public string TotalMidnightHours { get; set; } = "0:00";

        [Column("total_holiday_work_hours")]
        [MaxLength(10)]
        public string TotalHolidayWorkHours { get; set; } = "0:00";

        [Column("lock_status")]
        [MaxLength(20)]
        public string LockStatus { get; set; } = "未締め（編集可）";
    }
}