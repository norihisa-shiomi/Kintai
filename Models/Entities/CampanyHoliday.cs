using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KintaiSystem.Models.Entities
{
    [Table("company_holidays")]
    public class CompanyHoliday
    {
        [Key]
        [Column("holiday_date")]
        public DateTime HolidayDate { get; set; }

        [Required]
        [Column("holiday_name")]
        [MaxLength(50)]
        public string HolidayName { get; set; } = string.Empty;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}