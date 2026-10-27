// 対象社員ID（固定値、またはログイン情報から取得する仕様）
const currentEmployeeId = 1;

/**
 * サーバーの月次勤怠APIを呼び出し、画面に描画する
 */
async function loadMonthlyData() {
    const monthPicker = document.getElementById("monthPicker");
    if (!monthPicker) return;

    const selectedMonth = monthPicker.value || "2026-10";
    const [year, month] = selectedMonth.split("-").map(Number);

    try {
        // バックエンドの AttendanceController (GET /api/attendance/monthly) を呼び出し
        const response = await fetch(`/api/attendance/monthly?employeeId=${currentEmployeeId}&year=${year}&month=${month}`);
        
        if (!response.ok) {
            throw new Error(`APIエラー: ステータス ${response.status}`);
        }

        const data = await response.json();
        
        // 取得したデータを画面に反映
        renderView(data);

    } catch (error) {
        console.error("月次データの取得に失敗しました:", error);
        alert("月次データの取得に失敗しました。サーバーの起動状態を確認してください。");
    }
}

/**
 * APIから返ってきた MonthlyAttendanceResponseDto の内容を画面に描画
 */
function renderView(data) {
    // 1. 社員名・締め状態の反映
    const employeeNameElem = document.getElementById("employeeName");
    if (employeeNameElem && data.employeeName) {
        employeeNameElem.innerText = data.employeeName;
    }

    const lockStatusTextElem = document.getElementById("lockStatusText");
    const lockStatusBadgeElem = document.getElementById("lockStatusBadge");

    if (lockStatusTextElem) {
        lockStatusTextElem.innerText = data.lockStatus;
    }
    if (lockStatusBadgeElem) {
        lockStatusBadgeElem.className = data.isClosed ? "status-badge closed" : "status-badge open";
    }

    // 2. 上部サマリー（合計日数・合計時間）の反映
    document.getElementById("summaryWorkDays").innerText = `${data.workDays} 日`;
    document.getElementById("summaryHolidayDays").innerText = `${data.holidayWorkDays} 日`;
    document.getElementById("summaryTotalHours").innerText = data.totalWorkHours;
    document.getElementById("summaryOvertime").innerText = data.totalOvertimeHours;
    document.getElementById("summaryMidnight").innerText = data.totalMidnightHours;
    document.getElementById("summaryHolidayHours").innerText = data.totalHolidayWorkHours;

    // 3. テーブル（一覧明細）の描画
    const tbody = document.getElementById("attendanceTableBody");
    if (!tbody) return;

    tbody.innerHTML = "";

    const isClosed = data.isClosed;
    const disabledAttr = isClosed ? "disabled title='締め完了済みのため修正申請不可'" : "";

    data.dailyList.forEach(item => {
        const tr = document.createElement("tr");

        // 土日の色分け
        if (item.dayOfWeek === "土") tr.className = "sat";
        if (item.dayOfWeek === "日") tr.className = "sun";

        tr.innerHTML = `
            <td>${item.displayDate}</td>
            <td>${item.dayOfWeek}</td>
            <td>${item.clockInTime || "-"}</td>
            <td>${item.clockOutTime || "-"}</td>
            <td>${item.breakStartTime || "-"}</td>
            <td>${item.breakEndTime || "-"}</td>
            <td><strong>${item.workHours || "0:00"}</strong></td>
            <td>${item.overtimeHours || "0:00"}</td>
            <td>${item.midnightHours || "0:00"}</td>
            <td>${item.holidayWorkHours || "0:00"}</td>
            <td>
                <button class="btn-apply" ${disabledAttr} onclick="goToApply('${item.workDate}')">
                    修正申請
                </button>
            </td>
        `;
        tbody.appendChild(tr);
    });
}

/**
 * 年月セレクター変更時イベント
 */
function updateMonthlyData() {
    loadMonthlyData();
}

/**
 * 修正申請画面への遷移
 */
function goToApply(dateStr) {
    alert(`${dateStr} の修正申請画面（SC-07）へ遷移します。`);
}

// 画面読み込み完了時に実行
document.addEventListener("DOMContentLoaded", () => {
    loadMonthlyData();
});