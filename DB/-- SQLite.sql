-- SQLite
BEGIN TRANSACTION;

-- 1. 紐づく打刻データ（子テーブル）を先に削除
DELETE FROM time_stamps 
WHERE daily_attendance_id BETWEEN 154 AND 190;

-- 2. 勤怠日次データ（親テーブル）を削除
DELETE FROM daily_attendances 
WHERE id BETWEEN 154 AND 190;

COMMIT;