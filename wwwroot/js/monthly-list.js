// 既存の打刻・実績データのサンプル（DBから取得するイメージ）
const attendanceRecords = {
    "2026-10-01": { in: "09:00", out: "18:00", bStart: "12:00", bEnd: "13:00", work: "8:00", over: "0:00", mid: "0:00", hol: "0:00" },
    "2026-10-02": { in: "09:00", out: "18:00", bStart: "12:00", bEnd: "13:00", work: "8:00", over: "0:00", mid: "0:00", hol: "0:00" },
    "2026-10-05": { in: "09:00", out: "18:00", bStart: "12:00", bEnd: "13:00", work: "8:00", over: "0:00", mid: "0:00", hol: "0:00" },
    "2026-10-13": { in: "09:00", out: "18:01", bStart: "12:00", bEnd: "13:00", work: "8:01", over: "0:01", mid: "0:00", hol: "0:00" },
    "2026-10-15": { in: "13:00", out: "22:01", bStart: "17:00", bEnd: "18:00", work: "8:01", over: "0:01", mid: "0:01", hol: "0:00" },
    "2026-10-24": { in: "09:00", out: "19:00", bStart: "12:00", bEnd: "13:00", work: "9:00", over: "0:00", mid: "0:00", hol: "9:00" },
    "2026-10-25": { in: "18:00", out: "02:00", bStart: "21:00", bEnd: "22:00", work: "7:00", over: "0:00", mid: "4:00", hol: "7:00" },
};

const weekDays = ["日", "月", "火", "水", "木", "金", "土"];
let isClosedMonth = false;

// "HH:mm" 形式の文字列を「分」に変換する関数
function parseTimeToMinutes(timeStr) {
    if (!timeStr || timeStr === "0:00" || timeStr === "-") return 0;
    const [hours, minutes] = timeStr.split(":").map(Number);
    return hours * 60 + minutes;
}

// 「分」を "H:mm" 形式の文字列に変換する関数
function formatMinutesToTime(totalMinutes) {
    const hours = Math.floor(totalMinutes / 60);
    const minutes = totalMinutes % 60;
    return `${hours}:${String(minutes).padStart(2, "0")}`;
}

/**
 * テーブル描画 ＆ 月次集計（日数・各時間）の自動計算
 */
function renderMonthlyTable(yearMonthStr) {
    const tbody = document.getElementById("attendanceTableBody");
    if (!tbody) return;
    
    tbody.innerHTML = "";

    const [year, month] = yearMonthStr.split("-").map(Number);
    const daysInMonth = new Date(year, month, 0).getDate();

    // 集計用カウンターの初期化
    let workDaysCount = 0;         // 出勤日数（平日）
    let holidayDaysCount = 0;      // 休日出勤日数
    let totalWorkMinutes = 0;      // 総労働時間（分）
    let totalOvertimeMinutes = 0;  // 総残業時間（分）
    let totalMidnightMinutes = 0;  // 総深夜時間（分）
    let totalHolidayMinutes = 0;   // 総休日労働時間（分）

    for (let day = 1; day <= daysInMonth; day++) {
        const dateStr = `${year}-${String(month).padStart(2, '0')}-${String(day).padStart(2, '0')}`;
        const dateObj = new Date(year, month - 1, day);
        const dayOfWeekIndex = dateObj.getDay(); // 0:日, 6:土
        const dayOfWeekStr = weekDays[dayOfWeekIndex];
        const isWeekend = (dayOfWeekIndex === 0 || dayOfWeekIndex === 6);

        const record = attendanceRecords[dateStr] || {};
        const hasWorked = !!(record.in || record.out); // 出勤実績があるか

        // --- 自動カウント＆時間の集計ロジック（設計書 第6章・6-3準拠） ---
        if (hasWorked) {
            if (isWeekend) {
                holidayDaysCount++; // 休日出勤日数
            } else {
                workDaysCount++;    // 平日出勤日数
            }
        }

        // 各時間の合計（分）を加算
        totalWorkMinutes += parseTimeToMinutes(record.work);
        totalOvertimeMinutes += parseTimeToMinutes(record.over);
        totalMidnightMinutes += parseTimeToMinutes(record.mid);
        totalHolidayMinutes += parseTimeToMinutes(record.hol);

        // --- 行の生成 ---
        const tr = document.createElement("tr");
        if (dayOfWeekIndex === 6) tr.className = "sat";
        if (dayOfWeekIndex === 0) tr.className = "sun";

        const displayDate = `${String(month).padStart(2, '0')}/${String(day).padStart(2, '0')}`;
        const disabledAttr = isClosedMonth ? "disabled title='締め完了済みのため修正申請不可'" : "";

        tr.innerHTML = `
            <td>${displayDate}</td>
            <td>${dayOfWeekStr}</td>
            <td>${record.in || "-"}</td>
            <td>${record.out || "-"}</td>
            <td>${record.bStart || "-"}</td>
            <td>${record.bEnd || "-"}</td>
            <td><strong>${record.work || "0:00"}</strong></td>
            <td>${record.over || "0:00"}</td>
            <td>${record.mid || "0:00"}</td>
            <td>${record.hol || "0:00"}</td>
            <td>
                <button class="btn-apply" ${disabledAttr} onclick="goToApply('${dateStr}')">
                    修正申請
                </button>
            </td>
        `;
        tbody.appendChild(tr);
    }

    // --- 画面上部サマリーへの自動反映 (SC-05-12〜17) ---
    document.getElementById("summaryWorkDays").innerText = `${workDaysCount} 日`;
    document.getElementById("summaryHolidayDays").innerText = `${holidayDaysCount} 日`;
    document.getElementById("summaryTotalHours").innerText = formatMinutesToTime(totalWorkMinutes);
    document.getElementById("summaryOvertime").innerText = formatMinutesToTime(totalOvertimeMinutes);
    document.getElementById("summaryMidnight").innerText = formatMinutesToTime(totalMidnightMinutes);
    document.getElementById("summaryHolidayHours").innerText = formatMinutesToTime(totalHolidayMinutes);
}

function goToApply(dateStr) {
    alert(`${dateStr} の修正申請画面（SC-07）へ遷移します。`);
}

function updateMonthlyData() {
    const selectedMonth = document.getElementById("monthPicker").value;
    
    if (selectedMonth < "2026-10") {
        isClosedMonth = true;
        document.getElementById("lockStatusText").innerText = "締め済み（ロック）";
        document.getElementById("lockStatusBadge").className = "status-badge closed";
    } else {
        isClosedMonth = false;
        document.getElementById("lockStatusText").innerText = "未締め（編集可）";
        document.getElementById("lockStatusBadge").className = "status-badge open";
    }

    renderMonthlyTable(selectedMonth);
}

document.addEventListener("DOMContentLoaded", () => {
    const defaultMonth = document.getElementById("monthPicker").value || "2026-10";
    renderMonthlyTable(defaultMonth);
});