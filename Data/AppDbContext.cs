using Microsoft.EntityFrameworkCore;
using KintaiSystem.Models.Entities;

namespace KintaiSystem.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // テーブルの定義
        public DbSet<Employee> Employees { get; set; } = null!;
        public DbSet<DailyAttendance> DailyAttendances { get; set; } = null!;
        public DbSet<BreakTime> BreakTimes { get; set; } = null!;
        public DbSet<TimeStamp> TimeStamps { get; set; } = null!;
        public DbSet<CompanyHoliday> CompanyHolidays { get; set; }
        public DbSet<MonthlySummary> MonthlySummaries { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. employees テーブルのマッピング
            modelBuilder.Entity<Employee>(entity =>
            {
                entity.ToTable("employees");
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.EmployeeNumber).HasColumnName("employee_number");
                entity.Property(e => e.Name).HasColumnName("name");
                entity.Property(e => e.Email).HasColumnName("email");
                entity.Property(e => e.PasswordHash).HasColumnName("password_hash");
                entity.Property(e => e.IsAdmin).HasColumnName("is_admin");
                entity.Property(e => e.IsFirstLogin).HasColumnName("is_first_login");
            });

            // 2. daily_attendances テーブルのテーブル名だけをまず紐付ける
            modelBuilder.Entity<DailyAttendance>(entity =>
            {
            entity.Property(d => d.Id).HasColumnName("id");
            entity.Property(d => d.EmployeeId).HasColumnName("employee_id");
            entity.Property(d => d.WorkDate).HasColumnName("work_date");
            entity.Property(d => d.DayOfWeek).HasColumnName("day_of_week");
            entity.Property(d => d.ClockInTime).HasColumnName("clock_in_time");
            entity.Property(d => d.ClockOutTime).HasColumnName("clock_out_time");
            entity.Property(d => d.CurrentStatus).HasColumnName("current_status");
            });


            // 3. リレーションシップの設定（既存の記述そのまま）
            modelBuilder.Entity<DailyAttendance>()
                .HasMany(d => d.BreakTimes)
                .WithOne(b => b.DailyAttendance)
                .HasForeignKey(b => b.DailyAttendanceId);

            modelBuilder.Entity<DailyAttendance>()
                .HasMany(d => d.TimeStamps)
                .WithOne(t => t.DailyAttendance)
                .HasForeignKey(t => t.DailyAttendanceId);

            // time_stamps テーブルと各カラム名の物理マッピング
            modelBuilder.Entity<TimeStamp>(entity =>
            {
                entity.ToTable("time_stamps");
                entity.Property(t => t.Id).HasColumnName("id");
                entity.Property(t => t.EmployeeId).HasColumnName("employee_id");
                entity.Property(t => t.DailyAttendanceId).HasColumnName("daily_attendance_id");
                entity.Property(t => t.StampedAt).HasColumnName("stamped_at");
                entity.Property(t => t.StampType).HasColumnName("stamp_type");
            });
        }
    }
}
