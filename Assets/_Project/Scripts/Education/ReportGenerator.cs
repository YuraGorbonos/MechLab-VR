using System;
using System.IO;
using System.Text;
using SkillForge.Core;
using UnityEngine;

namespace SkillForge.Education
{
    public class ReportGenerator : IReportGenerator
    {
        public string Generate(SessionReport report)
        {
            var dir = Path.Combine(Application.persistentDataPath, "Reports");
            Directory.CreateDirectory(dir);

            var filePath = Path.Combine(dir, DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".html");

            GenerateHtmlReport(report, filePath);

            Debug.Log($"[ReportGenerator] Report saved: {filePath}");
            return filePath;
        }

        private static void GenerateHtmlReport(SessionReport report, string filePath)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html lang='ru'>");
            sb.AppendLine("<head><meta charset='UTF-8'><title>Отчёт о сессии MechLab VR</title><style>");
            sb.AppendLine("body{font-family:Arial,sans-serif;margin:40px;color:#333}");
            sb.AppendLine("h1{color:#1a5276;border-bottom:2px solid #1a5276;padding-bottom:10px}");
            sb.AppendLine("h2{color:#2c3e50;margin-top:30px}");
            sb.AppendLine("table{border-collapse:collapse;width:100%;margin:10px 0}");
            sb.AppendLine("th,td{border:1px solid #ccc;padding:6px 10px;text-align:left;font-size:13px}");
            sb.AppendLine("th{background:#1a5276;color:#fff;font-weight:bold}");
            sb.AppendLine("tr.completed{background:#d5f5e3}");
            sb.AppendLine("tr.failed{background:#fadbd8}");
            sb.AppendLine("tr.action-ok{background:#fff}");
            sb.AppendLine("tr.action-err{background:#fadbd8}");
            sb.AppendLine(".info{margin:10px 0;font-size:14px;line-height:1.8}");
            sb.AppendLine(".info span{font-weight:bold}");
            sb.AppendLine("</style></head><body>");

            sb.AppendLine("<h1>Отчёт о сессии MechLab VR</h1>");

            sb.AppendLine("<div class='info'>");
            sb.AppendLine($"<p><span>Сценарий:</span> {HtmlEscape(report.scenarioId)}</p>");
            sb.AppendLine($"<p><span>Режим:</span> {report.mode}</p>");
            sb.AppendLine($"<p><span>Длительность:</span> {TimeSpan.FromSeconds(report.durationSeconds):mm\\:ss}</p>");
            sb.AppendLine($"<p><span>Выполнено шагов:</span> {report.completedSteps} / {report.totalSteps}</p>");
            var scoreColor = report.score >= 60 ? "green" : "red";
            sb.AppendLine($"<p><span>Оценка:</span> <span style='color:{scoreColor};font-weight:bold'>{report.score:F1}%</span> ({report.grade})</p>");
            sb.AppendLine("</div>");

            sb.AppendLine("<h2>Шаги</h2>");
            sb.AppendLine("<table><tr><th>Step ID</th><th>Инструкция</th><th>Выполнен</th><th>Ошибки</th></tr>");
            if (report.stepResults != null)
            {
                foreach (var step in report.stepResults)
                {
                    var cls = step.completed ? "completed" : "failed";
                    var done = step.completed ? "Да" : "Нет";
                    sb.AppendLine($"<tr class='{cls}'><td>{HtmlEscape(step.stepId)}</td><td>{HtmlEscape(step.instruction)}</td><td>{done}</td><td>{step.errorCount}</td></tr>");
                }
            }
            sb.AppendLine("</table>");

            sb.AppendLine("<h2>Журнал действий</h2>");
            sb.AppendLine("<table><tr><th>Время</th><th>Действие</th><th>Объект</th><th>Игрок</th><th>Успех</th><th>Ошибка</th></tr>");
            if (report.actionLog != null)
            {
                foreach (var entry in report.actionLog)
                {
                    var cls = entry.success ? "action-ok" : "action-err";
                    var ok = entry.success ? "✓" : "✗";
                    sb.AppendLine($"<tr class='{cls}'><td>{entry.timestamp:F1}</td><td>{HtmlEscape(entry.intentType)}</td><td>{HtmlEscape(entry.targetObject)}</td><td>P{entry.playerId}</td><td>{ok}</td><td>{HtmlEscape(entry.errorCode)}</td></tr>");
                }
            }
            sb.AppendLine("</table>");

            sb.AppendLine("<p style='color:#888;font-size:12px;margin-top:40px'>");
            sb.AppendLine($"Сгенерировано {DateTime.Now:dd.MM.yyyy HH:mm:ss}</p>");
            sb.AppendLine("</body></html>");

            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        }

        private static string HtmlEscape(string s)
        {
            if (string.IsNullOrEmpty(s)) return "-";
            return s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");
        }
    }
}
