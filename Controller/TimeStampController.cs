using System;
using Microsoft.AspNetCore.Mvc;
using KintaiSystem.Services;

namespace KintaiSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TimeStampController : ControllerBase
    {
        private readonly AttendanceService _attendanceService;

        public TimeStampController(AttendanceService attendanceService)
        {
            _attendanceService = attendanceService;
        }

        // SC-04: 現在の勤務状態と打刻履歴の取得 (GET: api/timestamp/status/1001)
        [HttpGet("status/{employeeId}")]
        public IActionResult GetStatus(int employeeId)
        {
            var (currentStatus, history) = _attendanceService.GetTodayStatus(employeeId);
            return Ok(new
            {
                currentStatus = currentStatus,
                history = history
            });
        }

        // SC-04: 打刻処理の実行 (POST: api/timestamp/stamp)
        [HttpPost("stamp")]
        public IActionResult Stamp([FromBody] StampRequestDto request)
        {
            if (request == null)
            {
                return BadRequest(new { success = false, message = "リクエストデータが空です。" });
            }

            var (isSuccess, message) = _attendanceService.ProcessStamp(
                request.EmployeeId,
                request.StampType,
                DateTime.Now
            );

            if (!isSuccess)
            {
                return BadRequest(new { success = false, message = message });
            }

            return Ok(new { success = true, message = message });
        }
    }

    // 打刻リクエスト用DTO
    public class StampRequestDto
    {
        public int EmployeeId { get; set; }
        public string StampType { get; set; }
    }
}