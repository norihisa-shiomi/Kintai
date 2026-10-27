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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // employees テーブルと各カラム名の物理マッピング
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

            // リレーションシップの設定
            modelBuilder.Entity<DailyAttendance>()
                .HasMany(d => d.BreakTimes)
                .WithOne(b => b.DailyAttendance)
                .HasForeignKey(b => b.DailyAttendanceId);

            modelBuilder.Entity<DailyAttendance>()
                .HasMany(d => d.TimeStamps)
                .WithOne(t => t.DailyAttendance)
                .HasForeignKey(t => t.DailyAttendanceId);
        }
    }
}