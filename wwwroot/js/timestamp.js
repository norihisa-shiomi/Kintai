document.addEventListener('DOMContentLoaded', () => {
    // 1. 1秒ごとに時計を更新
    updateClock();
    setInterval(updateClock, 1000);

    // 2. 画面初期化時に最新状態と履歴を取得
    loadCurrentStatus();

    // 3. イベントリスナーの設定
    document.getElementById('btn-clock-in').addEventListener('click', () => sendStamp('出勤'));
    document.getElementById('btn-clock-out').addEventListener('click', () => sendStamp('退勤'));
    document.getElementById('btn-break-start').addEventListener('click', () => sendStamp('休憩開始'));
    document.getElementById('btn-break-end').addEventListener('click', () => sendStamp('休憩終了'));
});

// 時計をリアルタイムで更新する関数
function updateClock() {
    const now = new Date();
    const formatted = now.getFullYear() + '/' +
        String(now.getMonth() + 1).padStart(2, '0') + '/' +
        String(now.getDate()).padStart(2, '0') + ' ' +
        String(now.getHours()).padStart(2, '0') + ':' +
        String(now.getMinutes()).padStart(2, '0') + ':' +
        String(now.getSeconds()).padStart(2, '0');

    const clockEl = document.getElementById('server-clock');
    if (clockEl) {
        clockEl.textContent = formatted;
    }
}

// サーバーから最新の勤務状態を取得して画面に反映する関数
async function loadCurrentStatus() {
    try {
        const response = await fetch('/api/timestamp/status');
        if (response.ok) {
            const data = await response.json();

            console.log('★サーバーから取得したデータ:', data);

            const userNameElem = document.getElementById('login-user-name');
            if (userNameElem && data.employeeName) {
                userNameElem.textContent = data.employeeName;
            }

            // サーバーから取得したステータスと履歴でUIを更新
            updateUI(data.currentStatus, data.history || []);
        } else {
            console.error('ステータス取得失敗:', response.status);
        }
    } catch (error) {
        console.error('通信エラー:', error);
    }
}

// 打刻APIを呼び出す関数
async function sendStamp(stampType) {
    hideAlerts();
    try {
        const response = await fetch('/api/timestamp/stamp', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                stampType: stampType
            })
        });

        const result = await response.json();

        if (response.ok && result.success) {
            showSuccess(result.message || '打刻が完了しました。');
            // 最新の状態・履歴に画面を更新
            loadCurrentStatus();
        } else {
            showError(result.message || '打刻処理でエラーが発生しました。');
        }
    } catch (error) {
        showError('通信エラーが発生しました。サーバーの状態を確認してください。');
    }
}

// 画面の表示状態（ボタン有効化/無効化・バッジ・打刻履歴）を更新する関数
function updateUI(status, history) {
    const badge = document.getElementById('status-badge');
    if (badge) {
        badge.textContent = status;
        badge.className = 'badge fs-5 px-3 py-2 ';
        switch (status) {
            case '勤務中': badge.classList.add('bg-success'); break;
            case '休憩中': badge.classList.add('bg-warning', 'text-dark'); break;
            case '退勤済み': badge.classList.add('bg-secondary'); break;
            default: badge.classList.add('bg-secondary'); break; // 勤務前
        }
    }

    // ボタンの押下可能・不可（disabled）制御
    document.getElementById('btn-clock-in').disabled = !(status === '勤務前');
    document.getElementById('btn-clock-out').disabled = (status !== '勤務中');
    document.getElementById('btn-break-start').disabled = (status !== '勤務中');
    document.getElementById('btn-break-end').disabled = (status !== '休憩中');

    // 当日打刻履歴の描画
    const historyBody = document.getElementById('history-list');
    if (historyBody) {
        if (history && history.length > 0) {
            historyBody.innerHTML = history.map(item => `
                <tr>
                    <td><span class="badge bg-info text-dark">${item.stampType}</span></td>
                    <td>${item.stampedAt}</td>
                </tr>
            `).join('');
        } else {
            historyBody.innerHTML = `
                <tr id="no-history-row">
                    <td colspan="2" class="text-muted py-3">本日の打刻データはありません</td>
                </tr>
            `;
        }
    }
}

function showSuccess(msg) {
    const el = document.getElementById('alert-success');
    if (el) {
        el.textContent = msg;
        el.classList.remove('d-none');
    }
}

function showError(msg) {
    const el = document.getElementById('alert-error');
    if (el) {
        el.textContent = msg;
        el.classList.remove('d-none');
    }
}

function hideAlerts() {
    const s = document.getElementById('alert-success');
    const e = document.getElementById('alert-error');
    if (s) s.classList.add('d-none');
    if (e) e.classList.add('d-none');
}