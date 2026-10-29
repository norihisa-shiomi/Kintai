using System;
using System.Security.Claims;
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

       // SC-04: 現在の勤務状態と打刻履歴の取得 (GET: api/timestamp/status)
        [HttpGet("status")]
        [HttpGet("status/{employeeId}")]
        public IActionResult GetStatus(int? employeeId)
        {
            int currentEmployeeId;
            string currentEmployeeName = "";

            var claimId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var claimName = User.FindFirst(ClaimTypes.Name)?.Value; // ← クッキーから名前を取得

            if (int.TryParse(claimId, out var authId))
            {
                currentEmployeeId = authId;
                currentEmployeeName = claimName ?? "";
            }
            else if (employeeId.HasValue)
            {
                currentEmployeeId = employeeId.Value;
            }
            else
            {
                return Unauthorized(new { success = false, message = "ログインが必要です。" });
            }

            var (currentStatus, history) = _attendanceService.GetTodayStatus(currentEmployeeId);
            return Ok(new
            {
                employeeId = currentEmployeeId,
                employeeName = currentEmployeeName, 
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

            // 認証情報からログイン中社員IDを取得（他人のID改ざんを防止）
            int targetEmployeeId;
            var claimValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (int.TryParse(claimValue, out var authId))
            {
                targetEmployeeId = authId; // ログイン社員のIDで強制上書き
            }
            else
            {
                targetEmployeeId = request.EmployeeId; // クッキーがない場合の予備
            }

            var (isSuccess, message) = _attendanceService.ProcessStamp(
                targetEmployeeId,
                request.StampType,
                DateTime.Now
            );

            if (!isSuccess)
            {
                return BadRequest(new { success = false, message = message });
            }

            return Ok(new { success = true, message = message });
        }

        // 打刻リクエスト用DTO
        public class StampRequestDto
        {
        public int EmployeeId { get; set; }
        public string StampType { get; set; }
        }
    }
}