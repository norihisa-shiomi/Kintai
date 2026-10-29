alert("monthly-list.js 読み込み成功");
// ログイン中の社員ID（初期状態はnull）
let currentEmployeeId = null;

/**
 * ログイン中ユーザー情報を取得してから月次データを読み込む
 */
async function initMonthlyPage() {
    try {
        // 打刻画面と同じステータス取得APIを呼び出し、ログイン中の社員IDと名前を取得
        const statusRes = await fetch('/api/timestamp/status');
        if (statusRes.ok) {
            const statusData = await statusRes.json();
            
            // サーバーから返ってきた社員IDを設定（プロパティ名が employeeId または id の場合に対応）
            currentEmployeeId = statusData.employeeId || statusData.id || 1;

            // ヘッダーのユーザー名を久遠さんに書き換え
            const loginUserElem = document.getElementById("loginUser");
            if (loginUserElem && statusData.employeeName) {
                const codeText = statusData.employeeCode ? ` (${statusData.employeeCode})` : "";
                loginUserElem.textContent = `${statusData.employeeName}${codeText}`;
            }
        }
    } catch (err) {
        console.error("ユーザー情報の取得に失敗しました:", err);
    }

    // 月次データを取得
    await loadMonthlyData();
}

/**
 * サーバーの月次勤怠APIを呼び出し、画面に描画する
 */
async function loadMonthlyData() {
    const monthPicker = document.getElementById("monthPicker");
    if (!monthPicker) return;

    // 現在の年月（未指定なら当月）
    const selectedMonth = monthPicker.value || new Date().toISOString().slice(0, 7);
    const [year, month] = selectedMonth.split("-").map(Number);

    if (!currentEmployeeId) {
        currentEmployeeId = 1; // フォールバック
    }

    try {
        // ログイン中の社員IDを使って月次一覧を取得
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
    // 1. 社員名・ヘッダーの反映
    const loginUserElem = document.getElementById("loginUser");
    if (loginUserElem && data.employeeName) {
        const codeText = data.employeeCode ? ` (${data.employeeCode})` : ""; 
        loginUserElem.textContent = `${data.employeeName}${codeText}`;
    }

    const employeeNameElem = document.getElementById("employeeName");
    if (employeeNameElem && data.employeeName) {
        employeeNameElem.innerText = data.employeeName;
    }

    const lockStatusTextElem = document.getElementById("lockStatusText");
    const lockStatusBadgeElem = document.getElementById("lockStatusBadge");

    if (lockStatusTextElem) {
        lockStatusTextElem.innerText = data.lockStatus || "未確定";
    }
    if (lockStatusBadgeElem) {
        lockStatusBadgeElem.className = data.isClosed ? "status-badge closed" : "status-badge open";
    }

    // 2. 上部サマリー（合計日数・合計時間）の反映
    document.getElementById("summaryWorkDays").innerText = `${data.workDays ?? 0} 日`;
    document.getElementById("summaryHolidayDays").innerText = `${data.holidayWorkDays ?? 0} 日`;
    document.getElementById("summaryTotalHours").innerText = data.totalWorkHours || "0:00";
    document.getElementById("summaryOvertime").innerText = data.totalOvertimeHours || "0:00";
    document.getElementById("summaryMidnight").innerText = data.totalMidnightHours || "0:00";
    document.getElementById("summaryHolidayHours").innerText = data.totalHolidayWorkHours || "0:00";

    // 3. テーブル（一覧明細）の描画
    const tbody = document.getElementById("attendanceTableBody");
    if (!tbody) return;

    tbody.innerHTML = "";

    const isClosed = data.isClosed;
    const disabledAttr = isClosed ? "disabled title='締め完了済みのため修正申請不可'" : "";

    if (data.dailyList && data.dailyList.length > 0) {
        data.dailyList.forEach(item => {
            const tr = document.createElement("tr");

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

// 画面読み込み完了時に初期化処理を実行
document.addEventListener("DOMContentLoaded", () => {
    initMonthlyPage();
});