document.addEventListener('DOMContentLoaded', () => {
    let currentStatus = '勤務前';
    let isAdmin = true;
    const todayHistory = [];

    const clockEl = document.getElementById('server-clock');
    const statusBadge = document.getElementById('status-badge');
    const alertSuccess = document.getElementById('alert-success');
    const alertError = document.getElementById('alert-error');
    const historyList = document.getElementById('history-list');

    const btnClockIn = document.getElementById('btn-clock-in');
    const btnClockOut = document.getElementById('btn-clock-out');
    const btnBreakStart = document.getElementById('btn-break-start');
    const btnBreakEnd = document.getElementById('btn-break-end');

    // 時計更新
    function updateClock() {
        const now = new Date();
        const year = now.getFullYear();
        const month = String(now.getMonth() + 1).padStart(2, '0');
        const date = String(now.getDate()).padStart(2, '0');
        const hours = String(now.getHours()).padStart(2, '0');
        const minutes = String(now.getMinutes()).padStart(2, '0');
        const seconds = String(now.getSeconds()).padStart(2, '0');
        clockEl.textContent = `${year}/${month}/${date} ${hours}:${minutes}:${seconds}`;
    }
    setInterval(updateClock, 1000);
    updateClock();

    // UI制御
    function updateUI() {
        statusBadge.textContent = currentStatus;
        
        statusBadge.className = 'badge fs-5 px-3 py-2 ';
        if (currentStatus === '勤務前') statusBadge.classList.add('bg-secondary');
        else if (currentStatus === '勤務中') statusBadge.classList.add('bg-success');
        else if (currentStatus === '休憩中') statusBadge.classList.add('bg-warning', 'text-dark');
        else if (currentStatus === '退勤済み') statusBadge.classList.add('bg-info');

        btnClockIn.disabled = !(currentStatus === '勤務前' || currentStatus === '退勤済み');
        btnClockOut.disabled = !(currentStatus === '勤務中');
        btnBreakStart.disabled = !(currentStatus === '勤務中');
        btnBreakEnd.disabled = !(currentStatus === '休憩中');

        document.querySelectorAll('.admin-only').forEach(el => {
            el.style.display = isAdmin ? 'inline-block' : 'none';
        });
    }

    function showSuccess(msg) {
        alertError.classList.add('d-none');
        alertSuccess.textContent = msg;
        alertSuccess.classList.remove('d-none');
    }

    function showError(msg) {
        alertSuccess.classList.add('d-none');
        alertError.textContent = msg;
        alertError.classList.remove('d-none');
    }

    function renderHistory() {
        if (todayHistory.length === 0) return;
        historyList.innerHTML = '';
        todayHistory.forEach(item => {
            const tr = document.createElement('tr');
            tr.innerHTML = `
                <td><span class="badge bg-light text-dark border">${item.type}</span></td>
                <td class="font-monospace">${item.time}</td>
            `;
            historyList.appendChild(tr);
        });
    }

    btnClockIn.addEventListener('click', () => {
        if (currentStatus !== '勤務前' && currentStatus !== '退勤済み') {
            showError('E-0010: 現在の勤務状態では出勤打刻は行えません。');
            return;
        }
        currentStatus = '勤務中';
        todayHistory.push({ type: '出勤', time: clockEl.textContent.split(' ')[1] });
        showSuccess('打刻に成功しました');
        updateUI();
        renderHistory();
    });

    btnClockOut.addEventListener('click', () => {
        if (currentStatus !== '勤務中') {
            showError('E-0012: 現在の勤務状態では退勤打刻は行えません。');
            return;
        }
        currentStatus = '退勤済み';
        todayHistory.push({ type: '退勤', time: clockEl.textContent.split(' ')[1] });
        showSuccess('打刻に成功しました');
        updateUI();
        renderHistory();
    });

    btnBreakStart.addEventListener('click', () => {
        if (currentStatus !== '勤務中') {
            showError('E-0014: 現在の勤務状態では休憩開始は行えません。');
            return;
        }
        currentStatus = '休憩中';
        todayHistory.push({ type: '休憩開始', time: clockEl.textContent.split(' ')[1] });
        showSuccess('打刻に成功しました');
        updateUI();
        renderHistory();
    });

    btnBreakEnd.addEventListener('click', () => {
        if (currentStatus !== '休憩中') {
            showError('E-0015: 現在の勤務状態では休憩終了は行えません。');
            return;
        }
        currentStatus = '勤務中';
        todayHistory.push({ type: '休憩終了', time: clockEl.textContent.split(' ')[1] });
        showSuccess('打刻に成功しました');
        updateUI();
        renderHistory();
    });

    updateUI();
});