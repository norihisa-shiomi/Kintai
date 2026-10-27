using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using KintaiSystem.Models.Dtos;
using KintaiSystem.Services.Interfaces;

namespace Kintai.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class AttendanceController : ControllerBase
    {
        private readonly IAttendanceService _attendanceService;

        public AttendanceController(IAttendanceService attendanceService)
        {
            _attendanceService = attendanceService;
        }

        /// <summary>
        /// SC-05 月次一覧データ取得API (設計書 3-2, 4-5)
        /// GET /api/attendance/monthly?employeeId={employeeId}&year={year}&month={month}
        /// </summary>
        [HttpGet("monthly")]
        public async Task<IActionResult> GetMonthlyAttendance([FromQuery] int employeeId, [FromQuery] int year, [FromQuery] int month)
        {
            // パラメータ検証（年・月の基本バリデーション）
            if (employeeId <= 0 || year < 2000 || month < 1 || month > 12)
            {
                return BadRequest(new { message = "指定されたパラメータ（社員ID、年、月）が不正です。" });
            }

            try
            {
                var result = await _attendanceService.GetMonthlyAttendanceAsync(employeeId, year, month);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "月次一覧の取得中にサーバーエラーが発生しました。", detail = ex.Message });
            }
        }
    }
}