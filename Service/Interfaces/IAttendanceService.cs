using System.Threading.Tasks;
using KintaiSystem.Models.Dtos;

namespace KintaiSystem.Services.Interfaces
{
    public interface IAttendanceService
    {
        /// <summary>
        /// SC-05 月次一覧データを取得・集計する
        /// </summary>
        /// <param name="employeeId">対象社員ID</param>
        /// <param name="year">対象年</param>
        /// <param name="month">対象月</param>
        /// <returns>画面表示用DTO</returns>
        Task<MonthlyAttendanceResponseDto> GetMonthlyAttendanceAsync(int employeeId, int year, int month);

        /// <summary>
        /// SC-09: 対象年月の全社員の月次勤怠データを集計し、DB保存（Upsert）およびDTO返却を行う
        /// </summary>
        /// <param name="targetYearMonth">対象年月 (YYYY-MM形式)</param>
        /// <returns>月次集計DTO</returns>
        Task<MonthlySummaryResponseDto> GetOrCalculateMonthlySummaryAsync(string targetYearMonth);
    }
}