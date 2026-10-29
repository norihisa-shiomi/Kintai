using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KintaiSystem.Models.Dtos;
using KintaiSystem.Services.Interfaces;

namespace KintaiSystem.Controllers
{
    [Authorize] // ログイン済みユーザーのみアクセス許可
    [Route("api/monthly-summary")]
    [ApiController]
    public class MonthlySummaryController : ControllerBase
    {
        private readonly IAttendanceService _attendanceService;

        // DI（依存性注入）により、先ほど作成した AttendanceService を受け取る
        public MonthlySummaryController(IAttendanceService attendanceService)
        {
            _attendanceService = attendanceService;
        }

        /// <summary>
        /// SC-09: 対象年月の月次勤怠集計データを取得する
        /// GET /api/monthly-summary?targetYearMonth=YYYY-MM
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetMonthlySummary([FromQuery] string? targetYearMonth)
        {
            // -------------------------------------------------------------
            // 1. 管理者権限チェック（外部設計書 第10章 F06 / 第9章 E-0022 準拠）
            // -------------------------------------------------------------
            var isAdminClaim = User.FindFirst("IsAdmin")?.Value ?? User.FindFirst(ClaimTypes.Role)?.Value;
            bool isAdmin = string.Equals(isAdminClaim, "True", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(isAdminClaim, "Admin", StringComparison.OrdinalIgnoreCase);
            
            if (!isAdmin)
            {
                // 一般社員の直接アクセス（URL直叩きやAPI呼び出し）は403で拒否
                return StatusCode(403, new { messageId = "E-0022", message = "アクセス権がありません。" });
        
            }


            // -------------------------------------------------------------
            // 2. 対象年月の未指定時補正（外部設計書 4-9 SC-09 No.2 初期値「当月」準拠）
            // -------------------------------------------------------------
            string yearMonth = string.IsNullOrWhiteSpace(targetYearMonth)
                ? DateTime.Now.ToString("yyyy-MM")
                : targetYearMonth;

            // -------------------------------------------------------------
            // 3. 年月フォーマットのバリデーション（外部設計書 第9章 E-0018 / 第1章 No.12 準拠）
            // -------------------------------------------------------------
            // 形式が "YYYY-MM" (7文字) かつ日付として正しく解釈できるかを検証
            if (!DateTime.TryParseExact(
                    yearMonth,
                    "yyyy-MM",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None,
                    out _))
            {
                return BadRequest(new { messageId = "E-0018", message = "対象年月は YYYY-MM 形式で指定してください。" });
            }
            // -------------------------------------------------------------
            // 4. サービス層の呼び出し ＆ 200 OK 返却
            // -------------------------------------------------------------
            var summaryData = await _attendanceService.GetOrCalculateMonthlySummaryAsync(yearMonth);
            return Ok(summaryData);
        }
    }
}