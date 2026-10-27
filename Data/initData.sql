-- ====================================================================
-- 勤怠管理システム データベース初期化＆全データ投入スクリプト
-- （既存テーブルをすべて削除してから再作成・データ投入を行います）
-- ====================================================================

-- --------------------------------------------------------------------
-- 第1部：既存テーブルの削除（初期化）
-- --------------------------------------------------------------------
DROP TABLE IF EXISTS attendance_audit_logs;
DROP TABLE IF EXISTS requested_break_times;
DROP TABLE IF EXISTS modification_requests;
DROP TABLE IF EXISTS monthly_closings;
DROP TABLE IF EXISTS monthly_summaries;
DROP TABLE IF EXISTS break_times;
DROP TABLE IF EXISTS daily_attendances;
DROP TABLE IF EXISTS time_stamps;
DROP TABLE IF EXISTS employees;
DROP TABLE IF EXISTS company_holidays;

-- --------------------------------------------------------------------
-- 第2部：テーブルの新規作成 (DDL)
-- --------------------------------------------------------------------

-- 1. 会社休日テーブル
CREATE TABLE company_holidays (
    holiday_date TEXT NOT NULL PRIMARY KEY, -- 休日日付 (YYYY-MM-DD)
    holiday_name TEXT NOT NULL,             -- 休日名称
    created_at   TEXT NOT NULL DEFAULT (DATETIME('now', 'localtime'))
);

-- 2. 社員テーブル
CREATE TABLE employees (
    id              INTEGER PRIMARY KEY AUTOINCREMENT,
    employee_number TEXT NOT NULL UNIQUE,   -- 社員番号 (5桁: E0001等)
    name            TEXT NOT NULL,          -- 氏名
    email           TEXT NOT NULL UNIQUE,   -- ログイン用メールアドレス
    password_hash   TEXT ,          -- 暗号化ハッシュ化保存
    is_admin        INTEGER NOT NULL DEFAULT 0, -- 管理者フラグ (0:一般, 1:管理者)
    is_first_login  INTEGER NOT NULL DEFAULT 1, -- 初回ログインフラグ (0:完了, 1:初回)
    created_at      TEXT NOT NULL DEFAULT (DATETIME('now', 'localtime')),
    updated_at      TEXT NOT NULL DEFAULT (DATETIME('now', 'localtime'))
);

-- 3. 打刻履歴テーブル
CREATE TABLE time_stamps (
    id                  INTEGER PRIMARY KEY AUTOINCREMENT,
    employee_id         INTEGER NOT NULL,
    daily_attendance_id INTEGER NOT NULL,
    stamped_at          TEXT NOT NULL, -- 打刻日時 (YYYY-MM-DD HH:mm:ss)
    stamp_type          TEXT NOT NULL, -- 種別 (clock_in / clock_out / break_start / break_end)
    FOREIGN KEY (employee_id) REFERENCES employees(id),
    FOREIGN KEY (daily_attendance_id) REFERENCES daily_attendances(id)
);

-- 4. 日次勤務テーブル
CREATE TABLE daily_attendances (
    id                   INTEGER PRIMARY KEY AUTOINCREMENT,
    employee_id          INTEGER NOT NULL,
    work_date            TEXT NOT NULL, -- 勤務日 (YYYY-MM-DD)
    day_of_week          TEXT NOT NULL, -- 曜日 (月/火/水/木/金/土/日)
    clock_in_time        TEXT,          -- 出勤時間 (HH:mm)
    clock_out_time       TEXT,          -- 退勤時間 (HH:mm)
    break_time           TEXT NOT NULL DEFAULT '00:00', -- 1日の休憩合計時間 (HH:mm)
    work_hours           TEXT NOT NULL DEFAULT '00:00', -- 1日の実労働時間 (HH:mm)
    overtime_hours       TEXT NOT NULL DEFAULT '00:00', -- 1日の残業時間 (HH:mm)
    midnight_hours       TEXT NOT NULL DEFAULT '00:00', -- 1日の深夜労働時間 (HH:mm)
    holiday_work_hours   TEXT NOT NULL DEFAULT '00:00', -- 1日の休日労働時間 (HH:mm)
    current_status       TEXT NOT NULL DEFAULT 'before_work', -- 状態 (before_work/working/on_break/left_work)
    updated_at           TEXT NOT NULL DEFAULT (DATETIME('now', 'localtime')),
    FOREIGN KEY (employee_id) REFERENCES employees(id)
);

-- 5. 休憩時間テーブル
CREATE TABLE break_times (
    id                  INTEGER PRIMARY KEY AUTOINCREMENT,
    daily_attendance_id INTEGER NOT NULL,
    break_start_time    TEXT NOT NULL, -- 休憩開始時刻 (HH:mm)
    break_end_time      TEXT,          -- 休憩終了時刻 (HH:mm)
    FOREIGN KEY (daily_attendance_id) REFERENCES daily_attendances(id)
);

-- 6. 月次集計テーブル
CREATE TABLE monthly_summaries (
    id                       INTEGER PRIMARY KEY AUTOINCREMENT,
    employee_id              INTEGER NOT NULL,
    target_year_month        TEXT NOT NULL, -- 対象年月 (YYYY-MM)
    work_days                INTEGER NOT NULL DEFAULT 0, -- 出勤日数
    holiday_work_days        INTEGER NOT NULL DEFAULT 0, -- 休日出勤日数
    total_work_hours         TEXT NOT NULL DEFAULT '0:00', -- 月間総労働時間 (H:mm)
    total_overtime_hours     TEXT NOT NULL DEFAULT '0:00', -- 月間総残業時間 (H:mm)
    total_midnight_hours     TEXT NOT NULL DEFAULT '0:00', -- 月間総深夜時間 (H:mm)
    total_holiday_work_hours TEXT NOT NULL DEFAULT '0:00', -- 月間総休日労働時間 (H:mm)
    lock_status              TEXT NOT NULL DEFAULT '未締め', -- 締め状態
    FOREIGN KEY (employee_id) REFERENCES employees(id)
);

-- 7. 締め履歴テーブル
CREATE TABLE monthly_closings (
    id                INTEGER PRIMARY KEY AUTOINCREMENT,
    target_year_month TEXT NOT NULL UNIQUE, -- 対象年月 (YYYY-MM)
    unlocked_by       INTEGER,              -- 解除実行者ID (employees.id)
    unlocked_at       TEXT,                 -- 解除日時
    reason            TEXT,                 -- 解除理由
    FOREIGN KEY (unlocked_by) REFERENCES employees(id)
);

-- 8. 修正申請テーブル
CREATE TABLE modification_requests (
    id                      INTEGER PRIMARY KEY AUTOINCREMENT,
    employee_id             INTEGER NOT NULL, -- 申請者
    daily_attendance_id     INTEGER NOT NULL, -- 修正対象
    request_type            TEXT NOT NULL DEFAULT 'update', -- 申請区分 (update/create/delete)
    requested_clock_in_time TEXT,             -- 変更後出勤時刻 (HH:mm)
    requested_clock_out_time TEXT,            -- 変更後退勤時刻 (HH:mm)
    status                  TEXT NOT NULL DEFAULT 'pending', -- 状態 (pending/approved/rejected/canceled)
    approver_id             INTEGER,          -- 承認者/却下者 (employees.id)
    approved_at             TEXT,             -- 承認/却下判定日時
    requested_at            TEXT NOT NULL DEFAULT (DATETIME('now', 'localtime')),
    reason                  TEXT NOT NULL,    -- 申請理由 (必須)
    rejection_reason        TEXT,             -- 却下理由 (却下時必須)
    FOREIGN KEY (employee_id) REFERENCES employees(id),
    FOREIGN KEY (daily_attendance_id) REFERENCES daily_attendances(id),
    FOREIGN KEY (approver_id) REFERENCES employees(id)
);

-- 9. 申請休憩時間テーブル
CREATE TABLE requested_break_times (
    id                      INTEGER PRIMARY KEY AUTOINCREMENT,
    modification_request_id INTEGER NOT NULL,
    break_start_time        TEXT NOT NULL, -- 変更後休憩開始時刻 (HH:mm)
    break_end_time          TEXT NOT NULL, -- 変更後休憩終了時刻 (HH:mm)
    FOREIGN KEY (modification_request_id) REFERENCES modification_requests(id)
);

-- 10. 変更監査ログテーブル
CREATE TABLE attendance_audit_logs (
    id                      INTEGER PRIMARY KEY AUTOINCREMENT,
    modification_request_id INTEGER NOT NULL,
    daily_attendance_id     INTEGER NOT NULL,
    action_type             TEXT NOT NULL, -- 操作種別 (approved / rejected)
    before_values_json      TEXT NOT NULL, -- 変更前データ (JSON)
    after_values_json       TEXT NOT NULL, -- 変更後データ (JSON)
    performed_by            INTEGER NOT NULL, -- 操作実行者ID (employees.id)
    performed_at            TEXT NOT NULL DEFAULT (DATETIME('now', 'localtime')),
    FOREIGN KEY (modification_request_id) REFERENCES modification_requests(id),
    FOREIGN KEY (daily_attendance_id) REFERENCES daily_attendances(id),
    FOREIGN KEY (performed_by) REFERENCES employees(id)
);

-- --------------------------------------------------------------------
-- 第3部：初期データの投入 (DML)
-- --------------------------------------------------------------------

-- 1. 会社休日データ
INSERT OR REPLACE INTO company_holidays (holiday_date, holiday_name) VALUES
    ('2026-10-12', 'スポーツの日'),
    ('2026-11-03', '文化の日'),
    ('2026-11-23', '勤労感謝の日'),
    ('2026-10-03', '土曜日'),
    ('2026-10-04', '日曜日'),
    ('2026-10-10', '土曜日'),
    ('2026-10-11', '日曜日'),
    ('2026-10-25', '日曜日'),
    ('2026-10-17', '土曜日'),
    ('2026-10-18', '日曜日'),
    ('2026-10-24', '土曜日'),
    ('2026-10-25', '日曜日'),
    ('2026-10-31', '土曜日'),
    ('2026-11-01', '日曜日'),
    ('2026-11-07', '土曜日'),
    ('2026-11-08', '日曜日'),
    ('2026-11-14', '土曜日'),
    ('2026-11-15', '日曜日'),
    ('2026-11-21', '土曜日'),
    ('2026-11-22', '日曜日'),
    ('2026-11-28', '土曜日'),
    ('2026-11-29', '日曜日'),
    ('2026-12-05', '土曜日'),
    ('2026-12-06', '日曜日'),
    ('2026-12-12', '土曜日'),
    ('2026-12-13', '日曜日'),
    ('2026-12-19', '土曜日'),
    ('2026-12-20', '日曜日'),
    ('2026-12-29', '年末年始'),
    ('2026-12-30', '年末年始'),
    ('2026-12-31', '年末年始'),
    ('2027-01-01', '年末年始'),
    ('2027-01-02', '年末年始'),
    ('2027-01-03', '年末年始');

-- 2. 社員マスターデータ
INSERT OR REPLACE INTO employees (id, employee_number, name, email, password_hash, is_admin, is_first_login) VALUES
    (1, 'E0001', '小田切 誠',   'odagiri@tsubame-note.example',       '', 1, 1),
    (2, 'E0002', '浅見 さやか', 'asami@tsubame-note.example',         '', 1, 1),
    (3, 'E0003', '久遠 拓海',   'kuon@tsubame-note.example',          '', 0, 1),
    (4, 'E0004', '早乙女 陽菜', 'saotome@tsubame-note.example',       '', 0, 1),
    (5, 'E0005', '鳴海 大地',   'narumi@tsubame-note.example',        '', 0, 1);


-- 3. 日次勤務 & 休憩時間データ（社員ID: 2 用）
INSERT OR REPLACE INTO daily_attendances (id, employee_id, work_date, day_of_week, clock_in_time, clock_out_time, break_time, work_hours, overtime_hours, midnight_hours, holiday_work_hours, current_status) VALUES
    (1, 2, '2026-10-05', '月', '09:00', '18:00', '01:00', '08:00', '00:00', '00:00', '00:00', 'left_work'),
    (2, 2, '2026-10-06', '火', '09:00', '15:00', '00:00', '06:00', '00:00', '00:00', '00:00', 'left_work'),
    (3, 2, '2026-10-07', '水', '09:00', '15:01', '00:01', '06:00', '00:00', '00:00', '00:00', 'left_work'),
    (4, 2, '2026-10-13', '火', '09:00', '18:01', '01:00', '08:00', '00:01', '00:00', '00:00', 'left_work'),
    (5, 2, '2026-10-23', '金', '09:00', '02:00', '01:00', '08:00', '08:00', '04:00', '00:00', 'left_work'),
    (6, 2, '2026-10-24', '土', '09:00', '18:00', '01:00', '00:00', '00:00', '00:00', '08:00', 'left_work');

INSERT OR REPLACE INTO break_times (id, daily_attendance_id, break_start_time, break_end_time) VALUES 
    (1, 1, '12:00', '13:00'),
    (2, 4, '12:00', '13:00'),
    (3, 5, '12:00', '13:00'),
    (4, 6, '12:00', '13:00');

-- 4. 月次集計初期データ (2026-10)
INSERT OR REPLACE INTO monthly_summaries (id, employee_id, target_year_month, work_days, holiday_work_days, total_work_hours, total_overtime_hours, total_midnight_hours, total_holiday_work_hours, lock_status) VALUES
    (1, 1, '2026-10', 20, 0, '160:00', '00:00', '00:00', '00:00', '締め済み'),
    (2, 2, '2026-10', 20, 1, '168:30', '08:15', '04:00', '08:00', '締め済み'),
    (3, 3, '2026-10', 21, 0, '168:00', '00:00', '00:00', '00:00', '締め済み'),
    (4, 4, '2026-10',  0, 0,   '0:00',  '0:00',  '0:00',  '0:00', '締め済み');

-- 5. 月次締め初期データ (2026-10)
INSERT OR REPLACE INTO monthly_closings (id, target_year_month) VALUES
    (1, '2026-10');

-- 6. 修正申請テストデータ
INSERT OR REPLACE INTO modification_requests (id, employee_id, daily_attendance_id, request_type, requested_clock_in_time, requested_clock_out_time, status, reason) VALUES
    (1, 2, 1, 'update', '09:00', '19:00', 'pending', '打刻漏れのため1時間残業を申請します');

INSERT OR REPLACE INTO requested_break_times (id, modification_request_id, break_start_time, break_end_time) VALUES
    (1, 1, '12:00', '13:00');
    