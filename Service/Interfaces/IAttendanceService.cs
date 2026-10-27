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
    }
}