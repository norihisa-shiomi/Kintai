using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using KintaiSystem.Services;

namespace KintaiSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CsvExportController : ControllerBase
    {
        private readonly KintaiCsvService _csvService;
        private readonly IKintaiRepository _repository; // DBアクセス用リポジトリ想定

        public CsvExportController(KintaiCsvService csvService, IKintaiRepository repository)
        {
            _csvService = csvService;
            _repository = repository;
        }

        /// <summary>
        /// 指定された対象年月の給与計算用CSVを出力します (SC-11)
        /// </summary>
        /// <param name="targetYearMonth">対象年月 (形式: YYYY-MM 例: 2026-10)</param>
        [HttpGet("download")]
        public async Task<IActionResult> DownloadSalaryCsv([FromQuery] string targetYearMonth)
        {
            try
            {
                // 1. 管理者権限チェック (第10章 F08 / 11-4)
                bool isAdmin = User.IsInRole("Admin"); // セッション/トークン情報より判定
                if (!isAdmin)
                {
                    // E-0024: アクセス権がありません。
                    return StatusCode(403, new { messageId = "E-0024", message = "アクセス権がありません。" });
                }

                // 2. 対象月の月締め状態チェック (前提 No.12 / 11-3 / SC-11)
                bool isClosed = await _repository.IsMonthClosedAsync(targetYearMonth);
                if (!isClosed)
                {
                    // E-0020: 該当月はすでに締め処理が完了していない、または未締めエラー
                    return BadRequest(new { messageId = "E-0020", message = "対象年月が締め完了していないため、CSVを出力できません。締め処理を行ってください。" });
                }

                // 3. 全社員の集計データを取得（全社員を取得・社員番号昇順・勤務なし社員は0詰め） (前提 No.13)
                var summaries = await _repository.GetMonthlySummariesForCsvAsync(targetYearMonth);

                // 4. CSVデータ生成 (UTF-8 BOM付き, CRLF)
                byte[] csvBytes = _csvService.GenerateSalaryCsv(summaries);

                // 5. ファイル名作成: kintai_202610.csv (前提 No.13 / 7-1)
                string formattedYearMonth = targetYearMonth.Replace("-", ""); // "2026-10" -> "202610"
                string fileName = $"kintai_{formattedYearMonth}.csv";

                // Content-Type: text/csv でダウンロードレスポンス返却
                return File(csvBytes, "text/csv; charset=utf-8", fileName);
            }
            catch (Exception ex)
            {
                // E-0026 / E-0025: CSV出力エラー
                return StatusCode(500, new { messageId = "E-0026", message = "CSV出力処理中にエラーが発生しました。" });
            }
        }
    }
}
