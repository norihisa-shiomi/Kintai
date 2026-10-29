-- ============================================================================
-- 1. 勤務1件の計算テストデータ (全37+9+2件 / 対象: 社員コード E0003)
-- ============================================================================
INSERT INTO daily_attendances (
    employee_id,
    work_date,
    day_of_week,
    clock_in_time,
    clock_out_time,
    break_time,
    work_hours, 
    overtime_hours,
    midnight_hours,
    holiday_work_hours,
    current_status
) VALUES
    -- K01
    ((SELECT id FROM employees WHERE Employee_Number = 'E0003'), '2026-10-05', '月', '09:00', '18:00', '01:00', '00:00', '00:00', '00:00', '00:00', 'left_work'),
    -- K02
    ((SELECT id FROM employees WHERE Employee_Number = 'E0003'), '2026-10-05', '月', '09:00', '12:00', '00:00', '00:00', '00:00', '00:00', '00:00', 'left_work'),
    -- K03
    ((SELECT id FROM employees WHERE Employee_Number = 'E0003'), '2026-10-05', '月', '09:00', '15:00', '00:00', '00:00', '00:00', '00:00', '00:00', 'left_work'),
    -- K04
    ((SELECT id FROM employees WHERE Employee_Number = 'E0003'), '2026-10-05', '月', '09:00', '15:01', '00:00', '00:00', '00:00', '00:00', '00:00', 'left_work'),
    -- K05
    ((SELECT id FROM employees WHERE Employee_Number = 'E0003'), '2026-10-05', '月', '09:00', '15:45', '00:00', '00:00', '00:00', '00:00', '00:00', 'left_work'),
    -- K06
    ((SELECT id FROM employees WHERE Employee_Number = 'E0003'), '2026-10-05', '月', '09:00', '15:46', '00:00', '00:00', '00:00', '00:00', '00:00', 'left_work'),
    -- K07
    ((SELECT id FROM employees WHERE Employee_Number = 'E0003'), '2026-10-05', '月', '09:00', '15:45', '00:45', '00:00', '00:00', '00:00', '00:00', 'left_work'),
    -- K08
    ((SELECT id FROM employees WHERE Employee_Number = 'E0003'), '2026-10-05', '月', '09:00', '16:00', '00:30', '00:00', '00:00', '00:00', '00:00', 'left_work'),
    -- K09
    ((SELECT id FROM employees WHERE Employee_Number = 'E0003'), '2026-10-05', '月', '09:00', '17:45', '00:00', '00:00', '00:00', '00:00', '00:00', 'left_work'),
    -- K10
    ((SELECT id FROM employees WHERE Employee_Number = 'E0003'), '2026-10-05', '月', '09:00', '17:45', '00:45', '00:00', '00:00', '00:00', '00:00', 'left_work'),
    -- K11
    ((SELECT id FROM employees WHERE Employee_Number = 'E0003'), '2026-10-05', '月', '09:00', '17:46', '00:45', '00:00', '00:00', '00:00', '00:00', 'left_work'),
    -- K12
    ((SELECT id FROM employees WHERE Employee_Number = 'E0003'), '2026-10-05', '月', '09:00', '18:00', '00:00', '00:00', '00:00', '00:00', '00:00', 'left_work'),
    -- K13
    ((SELECT id FROM employees WHERE Employee_Number = 'E0003'), '2026-10-05', '月', '09:00', '18:01', '01:00', '00:00', '00:00', '00:00', '00:00', 'left_work'),
    -- K14
    ((SELECT id FROM employees WHERE Employee_Number = 'E0003'), '2026-10-05', '月', '09:00', '19:00', '00:45', '00:00', '00:00', '00:00', '00:00', 'left_work'),
    -- K15
    ((SELECT id FROM employees WHERE Employee_Number = 'E0003'), '2026-10-05', '月', '09:00', '19:00', '01:00', '00:00', '00:00', '00:00', '00:00', 'left_work'),
    -- K16
    ((SELECT id FROM employees WHERE Employee_Number = 'E0003'), '2026-10-05', '月', '08:59:59', '18:00:30', '00:00', '00:00', '00:00', '00:00', '00:00', 'left_work'),
    -- K17
    ((SELECT id FROM employees WHERE Employee_Number = 'E0003'), '2026-10-05', '月', '09:00', '22:00', '01:00', '00:00', '00:00', '00:00', '00:00', 'left_work'),
    -- K18
    ((SELECT id FROM employees WHERE Employee_Number = 'E0003'), '2026-10-05', '月', '09:00', '22:01', '01:00', '00:00', '00:00', '00:00', '00:00', 'left_work'),
    -- K19
    ((SELECT id FROM employees WHERE Employee_Number = 'E0003'), '2026-10-05', '月', '09:00', '23:30', '01:00', '00:00', '00:00', '00:00', '00:00', 'left_work'),
    -- K20
    ((SELECT id FROM employees WHERE Employee_Number = 'E0003'), '2026-10-05', '月', '13:00', '翌02:00', '01:00', '00:00', '00:00', '00:00', '00:00', 'left_work'),
    -- K21
    ((SELECT id FROM employees WHERE Employee_Number = 'E0003'), '2026-10-05', '月', '22:00', '翌07:00', '01:00', '00:00', '00:00', '00:00', '00:00', 'left_work'),
    -- K22
    ((SELECT id FROM employees WHERE Employee_Number = 'E0003'), '2026-10-05', '月', '22:00', '翌07:00', '00:00', '00:00', '00:00', '00:00', '00:00', 'left_work'),
    -- K23
    ((SELECT id FROM employees WHERE Employee_Number = 'E0003'), '2026-10-05', '月', '22:00', '翌05:00', '00:00', '00:00', '00:00', '00:00', '00:00', 'left_work'),
    -- K24
    ((SELECT id FROM employees WHERE Employee_Number = 'E0003'), '2026-10-05', '月', '18:00', '翌03:00', '00:30', '00:00', '00:00', '00:00', '00:00', 'left_work'),
    -- K25
    ((SELECT id FROM employees WHERE Employee_Number = 'E0003'), '2026-10-05', '月', '04:00', '13:00', '01:00', '00:00', '00:00', '00:00', '00:00', 'left_work'),
    -- K26
    ((SELECT id FROM employees WHERE Employee_Number = 'E0003'), '2026-10-05', '月', '05:00', '14:00', '01:00', '00:00', '00:00', '00:00', '00:00', 'left_work'),
    -- K27
    ((SELECT id FROM employees WHERE Employee_Number = 'E0003'), '2026-10-05', '月', '04:59', '14:00', '01:00', '00:00', '00:00', '00:00', '00:00', 'left_work'),
    -- K28
    ((SELECT id FROM employees WHERE Employee_Number = 'E0003'), '2026-10-05', '月', '09:00', '翌08:59', '02:00', '00:00', '00:00', '00:00', '00:00', 'left_work'),
    -- K29
    ((SELECT id FROM employees WHERE Employee_Number = 'E0003'), '2026-10-10', '土', '09:00', '18:00', '01:00', '00:00', '00:00', '00:00', '00:00', 'left_work'),
    -- K30
    ((SELECT id FROM employees WHERE Employee_Number = 'E0003'), '2026-10-11', '日', '09:00', '20:00', '01:00', '00:00', '00:00', '00:00', '00:00', 'left_work'),
    -- K31
    ((SELECT id FROM employees WHERE Employee_Number = 'E0003'), '2026-10-12', '月', '09:00', '18:00', '01:00', '00:00', '00:00', '00:00', '00:00', 'left_work'),
    -- K32
    ((SELECT id FROM employees WHERE Employee_Number = 'E0003'), '2026-10-10', '土', '20:00', '翌02:00', '00:00', '00:00', '00:00', '00:00', '00:00', 'left_work'),
    -- K33
    ((SELECT id FROM employees WHERE Employee_Number = 'E0003'), '2026-10-09', '金', '20:00', '翌06:00', '01:00', '00:00', '00:00', '00:00', '00:00', 'left_work'),
    -- K34
    ((SELECT id FROM employees WHERE Employee_Number = 'E0003'), '2026-10-31', '土', '22:00', '翌06:00', '01:00', '00:00', '00:00', '00:00', '00:00', 'left_work'),
    -- K35
    ((SELECT id FROM employees WHERE Employee_Number = 'E0003'), '2026-12-29', '火', '09:00', '18:00', '01:00', '00:00', '00:00', '00:00', '00:00', 'left_work'),
    -- K36
    ((SELECT id FROM employees WHERE Employee_Number = 'E0003'), '2026-10-06', '火', '21:00', '翌06:00', '00:00', '00:00', '00:00', '00:00', '00:00', 'left_work'),
    -- K37
    ((SELECT id FROM employees WHERE Employee_Number = 'E0003'), '2026-10-05', '月', '09:00', '15:30', '00:00', '00:00', '00:00', '00:00', '00:00', 'left_work'),
    -- 2026-10-01 (木) 休憩1時間
    ((SELECT id FROM employees WHERE Employee_Number = 'E0003'), '2026-10-01', '木', '09:00', '18:00', '01:00', '00:00', '00:00', '00:00', '00:00', 'left_work'),
    -- 2026-10-02 (金) 休憩1時間
    ((SELECT id FROM employees WHERE Employee_Number = 'E0003'), '2026-10-02', '金', '09:00', '19:30', '01:00', '00:00', '00:00', '00:00', '00:00', 'left_work'),
    -- 2026-10-05 (月) 休憩なし
    ((SELECT id FROM employees WHERE Employee_Number = 'E0003'), '2026-10-05', '月', '09:00', '18:00', '00:00', '00:00', '00:00', '00:00', '00:00', 'left_work'),
    -- 2026-10-06 (火) 翌日未明退勤 / 休憩1時間
    ((SELECT id FROM employees WHERE Employee_Number = 'E0003'), '2026-10-06', '火', '13:00', '翌02:00', '01:00', '00:00', '00:00', '00:00', '00:00', 'left_work'),
    -- 2026-10-08 (木) 休憩なし
    ((SELECT id FROM employees WHERE Employee_Number = 'E0003'), '2026-10-08', '木', '10:00', '15:30', '00:00', '00:00', '00:00', '00:00', '00:00', 'left_work'),
    -- 2026-10-10 (土) 土曜出勤 / 休憩1時間
    ((SELECT id FROM employees WHERE Employee_Number = 'E0003'), '2026-10-10', '土', '09:00', '18:00', '01:00', '00:00', '00:00', '00:00', '00:00', 'left_work'),
    -- 2026-10-12 (月) 祝日・スポーツの日 / 休憩なし
    ((SELECT id FROM employees WHERE Employee_Number = 'E0003'), '2026-10-12', '月', '09:00', '14:00', '00:00', '00:00', '00:00', '00:00', '00:00', 'left_work'),
    -- 2026-10-13 (火) 休憩45分
    ((SELECT id FROM employees WHERE Employee_Number = 'E0003'), '2026-10-13', '火', '09:00', '17:46', '00:45', '00:00', '00:00', '00:00', '00:00', 'left_work'),
    -- 2026-10-30 (金) 夜勤〜翌朝 / 休憩1時間
    ((SELECT id FROM employees WHERE Employee_Number = 'E0003'), '2026-10-30', '金', '20:00', '翌06:00', '01:00', '00:00', '00:00', '00:00', '00:00', 'left_work'),
    -- 2026-10-31 (土) 土曜夜勤〜日曜朝 / 休憩1時間
    ((SELECT id FROM employees WHERE Employee_Number = 'E0003'), '2026-10-31', '土', '22:00', '翌06:00', '01:00', '00:00', '00:00', '00:00', '00:00', 'left_work'),
    -- 2026-10-01 (木) 休憩1時間
    ((SELECT id FROM employees WHERE Employee_Number = 'E0004'), '2026-10-01', '木', '09:00', '18:00', '01:00', '00:00', '00:00', '00:00', '00:00', 'left_work'),
    -- 2026-10-10 (土) 土曜出勤 / 休憩1時間
    ((SELECT id FROM employees WHERE Employee_Number = 'E0004'), '2026-10-10', '土', '09:00', '18:00', '01:00', '00:00', '00:00', '00:00', '00:00', 'left_work');
