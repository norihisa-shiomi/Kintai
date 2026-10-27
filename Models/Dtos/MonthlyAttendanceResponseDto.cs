using System.Collections.Generic;

namespace KintaiSystem.Models.Dtos
{
    /// <summary>
    /// SC-05 月次一覧 画面返却用DTO
    /// 設計書 4-5 SC-05 の項目を画面へ転送する
    /// </summary>
    public class MonthlyAttendanceResponseDto
    {
        // --- 画面上部ヘッダー情報 ---
        // SC-05 No.22: ログインユーザ名
        public string EmployeeName { get; set; } = string.Empty;

        // SC-05 No.1, 20: 表示年月 (形式: "YYYY-MM")
        public string TargetYearMonth { get; set; } = string.Empty;

        // SC-05 No.21: 締め状態表示 ("未締め（編集可）" または "締め済み（ロック）")
        public string LockStatus { get; set; } = "未締め（編集可）";

        // 締め制御フラグ (設計書 5-2: 締め済みのとき修正申請ボタン非活性判定用)
        public bool IsClosed { get; set; }

        // --- 月合計サマリー (SC-05 No.12〜17) ---
        // No.12: 出勤日数（月合計）
        public int WorkDays { get; set; }

        // No.13: 休日出勤日数（月合計）
        public int HolidayWorkDays { get; set; }

        // No.14: 労働時間（月合計、"H:mm" 形式）
        public string TotalWorkHours { get; set; } = "0:00";

        // No.15: 残業時間（月合計、"H:mm" 形式）
        public string TotalOvertimeHours { get; set; } = "0:00";

        // No.16: 深夜時間（月合計、"H:mm" 形式）
        public string TotalMidnightHours { get; set; } = "0:00";

        // No.17: 休日労働時間（月合計、"H:mm" 形式）
        public string TotalHolidayWorkHours { get; set; } = "0:00";

        // --- 月次明細テーブル行リスト (SC-05 No.2〜11) ---
        public List<DailyAttendanceDetailDto> DailyList { get; set; } = new();
    }

    /// <summary>
    /// 月次明細テーブルの1日分の行データ
    /// </summary>
    public class DailyAttendanceDetailDto
    {
        // 判定・リンク用キー ("YYYY-MM-DD")
        public string WorkDate { get; set; } = string.Empty;

        // SC-05 No.2: 日付 ("MM/DD" 形式)
        public string DisplayDate { get; set; } = string.Empty;

        // SC-05 No.3: 曜日 ("月", "火", "水", "木", "金", "土", "日")
        public string DayOfWeek { get; set; } = string.Empty;

        // 休日判定フラグ (土曜・日曜・会社休日の場合 true: 画面の色分け用)
        public bool IsHoliday { get; set; }

        // SC-05 No.4: 出勤時間 ("HH:mm"、未打刻時は null)
        public string? ClockInTime { get; set; }

        // SC-05 No.5: 退勤時間 ("HH:mm"、未打刻時は null)
        public string? ClockOutTime { get; set; }

        // SC-05 No.6: 休憩開始時間 ("HH:mm"、未打刻時は null)
        public string? BreakStartTime { get; set; }

        // SC-05 No.7: 休憩終了時間 ("HH:mm"、未打刻時は null)
        public string? BreakEndTime { get; set; }

        // SC-05 No.8: 労働時間 ("H:mm"、未計算時は "0:00")
        public string WorkHours { get; set; } = "0:00";

        // SC-05 No.9: 残業時間 ("H:mm"、未計算時は "0:00")
        public string OvertimeHours { get; set; } = "0:00";

        // SC-05 No.10: 深夜時間 ("H:mm"、未計算時は "0:00")
        public string MidnightHours { get; set; } = "0:00";

        // SC-05 No.11: 休日労働時間 ("H:mm"、未計算時は "0:00")
        public string HolidayWorkHours { get; set; } = "0:00";
    }
}