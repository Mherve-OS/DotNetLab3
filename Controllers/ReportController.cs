using System.Data;
using System.Text;
using StockTrack.Models;

namespace StockTrack.Controllers;

public class ReportController
{
    private readonly ReportRepository _repository;
    private readonly AuditRepository _auditRepo;

    public ReportController(ReportRepository repository, AuditRepository auditRepo)
    {
        _repository = repository;
        _auditRepo = auditRepo;
    }

    public DataTable GetCurrentStockReport() => _repository.GetCurrentStockReport();
    public DataTable GetStockMovementReport(DateTime? from = null, DateTime? to = null) =>
        _repository.GetStockMovementReport(from, to);
    public DataTable GetLowStockReport() => _repository.GetLowStockReport();
    public DataTable GetSupplierReport() => _repository.GetSupplierReport();

    public void ExportToCsv(DataTable table, string filePath, int currentUserId)
    {
        var sb = new StringBuilder();

        // Header row
        var columnNames = table.Columns.Cast<DataColumn>().Select(c => EscapeCsv(c.ColumnName));
        sb.AppendLine(string.Join(",", columnNames));

        // Data rows
        foreach (DataRow row in table.Rows)
        {
            var fields = row.ItemArray.Select(field => EscapeCsv(field?.ToString() ?? string.Empty));
            sb.AppendLine(string.Join(",", fields));
        }

        File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        _auditRepo.LogAction(currentUserId, "REPORT_EXPORT_CSV", $"Exported report to CSV file: {Path.GetFileName(filePath)}");
    }

    public void ExportToHtmlPdf(DataTable table, string filePath, string title, int currentUserId)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html><html><head><meta charset='utf-8'>");
        sb.AppendLine($"<title>{EscapeHtml(title)}</title>");
        sb.AppendLine("<style>");
        sb.AppendLine("body { font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; margin: 30px; color: #333; }");
        sb.AppendLine("h1 { color: #2c3e50; border-bottom: 2px solid #3498db; padding-bottom: 8px; }");
        sb.AppendLine(".meta { color: #7f8c8d; font-size: 13px; margin-bottom: 20px; }");
        sb.AppendLine("table { width: 100%; border-collapse: collapse; margin-top: 15px; }");
        sb.AppendLine("th, td { border: 1px solid #bdc3c7; padding: 8px 12px; text-align: left; font-size: 13px; }");
        sb.AppendLine("th { background-color: #2980b9; color: white; }");
        sb.AppendLine("tr:nth-child(even) { background-color: #f8f9fa; }");
        sb.AppendLine("@media print { body { margin: 0; } }");
        sb.AppendLine("</style></head><body>");

        sb.AppendLine($"<h1>StockTrack Report: {EscapeHtml(title)}</h1>");
        sb.AppendLine($"<div class='meta'>Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss} | Total Records: {table.Rows.Count}</div>");
        sb.AppendLine("<table><thead><tr>");

        foreach (DataColumn col in table.Columns)
        {
            sb.AppendLine($"<th>{EscapeHtml(col.ColumnName)}</th>");
        }
        sb.AppendLine("</tr></thead><tbody>");

        foreach (DataRow row in table.Rows)
        {
            sb.AppendLine("<tr>");
            foreach (var item in row.ItemArray)
            {
                sb.AppendLine($"<td>{EscapeHtml(item?.ToString() ?? string.Empty)}</td>");
            }
            sb.AppendLine("</tr>");
        }

        sb.AppendLine("</tbody></table></body></html>");

        File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        _auditRepo.LogAction(currentUserId, "REPORT_EXPORT_PDF_HTML", $"Exported report printable HTML/PDF to: {Path.GetFileName(filePath)}");
    }

    private static string EscapeCsv(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }
        return value;
    }

    private static string EscapeHtml(string text)
    {
        return System.Net.WebUtility.HtmlEncode(text);
    }
}
