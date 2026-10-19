using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using KintaiSystem.Models;

namespace KintaiSystem.Services
{
    public class KintaiCsvService
    {
        /// <summary>
        /// 給与計算ソフト連携用CSVバイト配列を生成します。
        /// </summary>
        /// <param name="summaries">全社員の月次集計リスト（社員番号昇順済）</param>
        /// <returns>UTF-8(BOM付き), CRLF改行のCSVデータ</returns>
        public byte[] GenerateSalaryCsv(IEnumerable<MonthlySummaryCsvDto> summaries)
        {
            using (var memoryStream = new MemoryStream())
            {
                // UTF-8 (BOM付き) のエンコーディング設定 (前提 No.13)
                var encoding = new UTF8Encoding(true);

                using (var writer = new StreamWriter(memoryStream, encoding, bufferSize: 1024, leaveOpen: true))
                {
                    // 改行コードを CRLF に固定 (前提 No.13 / 7-1)
                    writer.NewLine = "\r\n";

                    // 1. ヘッダー行を出力 (7-2)
                    writer.WriteLine("社員番号,氏名,対象年月,出勤日数,休日出勤日数,総労働時間,残業時間,深夜時間,休日労働時間");

                    // 2. データ行を出力
                    foreach (var item in summaries)
                    {
                        string line = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8}",
                            EscapeCsvCell(item.EmployeeNumber),
                            EscapeCsvCell(item.Name),
                            EscapeCsvCell(item.TargetYearMonth),
                            item.WorkDays,
                            item.HolidayWorkDays,
                            FormatMinutesToHHmm(item.TotalWorkMinutes),
                            FormatMinutesToHHmm(item.TotalOvertimeMinutes),
                            FormatMinutesToHHmm(item.TotalMidnightMinutes),
                            FormatMinutesToHHmm(item.TotalHolidayWorkMinutes)
                        );

                        writer.WriteLine(line);
                    }

                    writer.Flush();
                }

                return memoryStream.ToArray();
            }
        }

        /// <summary>
        /// 分単位の数値を "H:mm" 形式（分は2桁固定）に変換します。
        /// 例: 0 -> "0:00", 5 -> "0:05", 485 -> "8:05", 10350 -> "172:30"
        /// </summary>
        private string FormatMinutesToHHmm(int totalMinutes)
        {
            if (totalMinutes <= 0)
            {
                return "0:00";
            }

            int hours = totalMinutes / 60;
            int minutes = totalMinutes % 60;

            return $"{hours}:{minutes:D2}";
        }

        /// <summary>
        /// CSVのカンマやダブルクォーテーションをエスケープ処理します。
        /// </summary>
        private string EscapeCsvCell(string value)
        {
            if (string.IsNullOrEmpty(value)) return "\"\"";
            
            // ダブルクォーテーションのエスケープ (" -> "")
            string escaped = value.Replace("\"", "\"\"");
            return $"\"{escaped}\"";
        }
    }
}
