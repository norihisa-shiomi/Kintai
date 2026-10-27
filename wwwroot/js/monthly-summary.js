document.addEventListener("DOMContentLoaded", () => {
    const inputYearMonth = document.getElementById("targetYearMonth");
    const btnSearch = document.getElementById("btnSearch");
    const errorDiv = document.getElementById("errorMessage");

    // 1. 初期値として当月（例: 2026-10）をセット
    const today = new Date();
    const currentYear = today.getFullYear();
    const currentMonth = String(today.getMonth() + 1).padStart(2, "0");
    inputYearMonth.value = `${currentYear}-${currentMonth}`;

    // 2. イベントリスナー登録
    btnSearch.addEventListener("click", fetchMonthlySummary);

    // 3. 画面読み込み時に当月データを自動取得
    fetchMonthlySummary();

    /**
     * 月次集計APIを呼び出して画面を更新
     */
    async function fetchMonthlySummary() {
        const targetYM = inputYearMonth.value;
        errorDiv.style.display = "none";
        errorDiv.textContent = "";

        if (!targetYM) {
            showError("対象年月を指定してください。");
            return;
        }

        try {
            const response = await fetch(`/api/monthly-summary?targetYearMonth=${encodeURIComponent(targetYM)}`, {
                method: "GET",
                headers: {
                    "Accept": "application/json"
                }
            });

            if (!response.ok) {
                const errData = await response.json().catch(() => ({}));
                showError(errData.message || `エラーが発生しました (${response.status})`);
                return;
            }

            const data = await response.json();
            renderSummary(data);
        } catch (error) {
            showError("サーバーとの通信に失敗しました。サーバーが起動しているか確認してください。");
        }
    }

    /**
     * 取得した集計データを各DOM要素へ反映
     */
    function renderSummary(data) {
        // 締め状態バッジ
        const badge = document.getElementById("lockStatusBadge");
        badge.textContent = data.lockStatus || "未締め";
        badge.className = `badge-status ${data.isClosed ? "badge-success" : "badge-warning"}`;

        // 全社KPIサマリー
        document.getElementById("totalEmployeeCount").textContent = `${data.totalEmployeeCount} 名`;
        document.getElementById("overallTotalWorkHours").textContent = data.overallTotalWorkHours;
        document.getElementById("overallTotalOvertimeHours").textContent = data.overallTotalOvertimeHours;

        // 社員別明細一覧
        const tbody = document.getElementById("summaryTableBody");
        tbody.innerHTML = "";

        if (!data.employeeSummaries || data.employeeSummaries.length === 0) {
            tbody.innerHTML = `<tr><td colspan="9" class="text-center" style="padding: 2rem; color: #6c757d;">該当する社員データがありません。</td></tr>`;
            return;
        }

        data.employeeSummaries.forEach(emp => {
            const tr = document.createElement("tr");

            // 休憩自動補正フラグの警告バッジ
            const warningBadge = emp.hasBreakAdjustmentWarning
                ? `<span class="badge-status badge-danger" style="font-size: 0.75rem; padding: 2px 6px;">休憩自動補正</span>`
                : `<span style="color: #adb5bd;">-</span>`;

            // 残業時間がある場合は赤字強調
            const overtimeClass = emp.totalOvertimeHours !== "0:00" ? "text-danger font-bold" : "";

            tr.innerHTML = `
                <td>${escapeHtml(emp.employeeNumber)}</td>
                <td>${escapeHtml(emp.employeeName)}</td>
                <td class="text-right">${emp.workDays} 日</td>
                <td class="text-right">${emp.holidayWorkDays} 日</td>
                <td class="text-right font-bold">${emp.totalWorkHours}</td>
                <td class="text-right ${overtimeClass}">${emp.totalOvertimeHours}</td>
                <td class="text-right">${emp.totalMidnightHours}</td>
                <td class="text-right">${emp.totalHolidayWorkHours}</td>
                <td class="text-center">${warningBadge}</td>
            `;
            tbody.appendChild(tr);
        });
    }

    /**
     * エラーメッセージ表示
     */
    function showError(msg) {
        errorDiv.textContent = msg;
        errorDiv.style.display = "block";
    }

    /**
     * XSS対策用エスケープ関数
     */
    function escapeHtml(str) {
        if (!str) return "";
        return String(str).replace(/[&<>'"]/g, match => ({
            "&": "&amp;",
            "<": "&lt;",
            ">": "&gt;",
            "'": "&#39;",
            "\"": "&quot;"
        }[match] || match));
    }
});