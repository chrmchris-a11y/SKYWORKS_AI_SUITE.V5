using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Text;
using Skyworks.Core.Models.ARC;
using Skyworks.Core.Models.SAIL;

namespace Skyworks.Core.Services.Reports;

public interface ISAILReportService
{
    SAILReportResult GenerateReport(SAILReportRequest request);
}

public class SAILReportRequest
{
    public string SoraVersion { get; set; } = "2.5"; // "2.0" ή "2.5"
    public int FinalGRC { get; set; }
    public ARCRating ResidualARC { get; set; }
    public string? MissionId { get; set; }
    public string? MissionName { get; set; }
    public DateTime? TimestampUtc { get; set; }
    public List<OSOImplementation>? ImplementedOSOs { get; set; }
}

public class SAILReportResult
{
    public string Html { get; set; } = string.Empty;
    public byte[] Pdf { get; set; } = Array.Empty<byte>();
    public string FileName { get; set; } = "SAIL-Report.pdf";
}

public class SAILReportService : ISAILReportService
{
    private readonly ISAILComplianceService _compliance;

    public SAILReportService(ISAILComplianceService compliance)
    {
        _compliance = compliance;
        // QuestPDF License (Community)
        QuestPDF.Settings.License = LicenseType.Community;
        
        // Configure fallback font for Greek/international characters
        // QuestPDF uses system fonts by default (Arial Unicode MS on Windows, Liberation Sans on Linux)
        // For custom fonts, you would register like:
        // QuestPDF.Settings.DefaultFont = "Arial Unicode MS"; // or "Noto Sans"
    }

    public SAILReportResult GenerateReport(SAILReportRequest request)
    {
        var ts = request.TimestampUtc ?? DateTime.UtcNow;
        // 1) Compute compliance snapshot via existing Phase 4 Step 33 service
        var comp = _compliance.CheckCompliance(new SAILComplianceRequest
        {
            SoraVersion = request.SoraVersion,
            FinalGRC = request.FinalGRC,
            ResidualARC = request.ResidualARC,
            ImplementedOSOs = request.ImplementedOSOs ?? new List<OSOImplementation>()
        });

        // Compose simple exposure note for single-segment
        var exposureNote = $"Single-segment mission: 100% exposure at SAIL {comp.SAIL}.";

        // 2) Build HTML summary
        var html = BuildHtml(comp, request, ts, exposureNote);

        // 3) Build PDF via QuestPDF DSL
        var pdfBytes = BuildPdf(comp, request, ts, exposureNote);

        var fileName = $"SAIL-Report_{(request.MissionId ?? "mission")}_{ts:yyyyMMdd_HHmm}.pdf";
        return new SAILReportResult
        {
            Html = html,
            Pdf = pdfBytes,
            FileName = fileName
        };
    }

    private static string BuildHtml(SAILComplianceResult comp, SAILReportRequest req, DateTime ts, string exposure)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<html><head><meta charset='utf-8'><style>");
        sb.AppendLine("body{font-family:Arial,Helvetica,sans-serif;margin:24px;color:#222}");
        sb.AppendLine("h1{font-size:20px;margin-bottom:4px} h2{font-size:16px;margin-top:18px}");
        sb.AppendLine(".kv{margin:6px 0} .tag{display:inline-block;padding:2px 6px;border-radius:4px;background:#eee;margin-right:6px}");
        sb.AppendLine("table{border-collapse:collapse;margin-top:8px} td,th{border:1px solid #ddd;padding:6px}");
        sb.AppendLine("</style></head><body>");

        sb.AppendLine("<h1>SAIL Compliance Report</h1>");
        sb.AppendLine($"<div class='kv'><span class='tag'>SORA {req.SoraVersion}</span><span class='tag'>UTC {ts:yyyy-MM-dd HH:mm}</span></div>");
        if (!string.IsNullOrWhiteSpace(req.MissionName)) sb.AppendLine($"<div class='kv'><b>Mission:</b> {System.Web.HttpUtility.HtmlEncode(req.MissionName)}</div>");
        if (!string.IsNullOrWhiteSpace(req.MissionId)) sb.AppendLine($"<div class='kv'><b>Mission ID:</b> {System.Web.HttpUtility.HtmlEncode(req.MissionId)}</div>");

        sb.AppendLine("<h2>Αποτελέσματα</h2>");
        sb.AppendLine("<table>");
        sb.AppendLine($"<tr><th>Final GRC</th><td>{req.FinalGRC}</td></tr>");
        sb.AppendLine($"<tr><th>Residual ARC</th><td>{req.ResidualARC}</td></tr>");
        sb.AppendLine($"<tr><th>SAIL</th><td>{comp.SAIL}</td></tr>");
        sb.AppendLine($"<tr><th>TMPR</th><td>{comp.TMPR?.Level} ({comp.TMPR?.Robustness})</td></tr>");
        sb.AppendLine($"<tr><th>Compliance</th><td>{(comp.IsCompliant ? "COMPLIANT" : "NON-COMPLIANT")}</td></tr>");
        sb.AppendLine($"<tr><th>Exposure</th><td>{System.Web.HttpUtility.HtmlEncode(exposure)}</td></tr>");
        sb.AppendLine("</table>");

        sb.AppendLine("<h2>OSO Συμμόρφωση</h2>");
        sb.AppendLine("<ul>");
        if (comp.OSOCompliance != null)
        {
            if (comp.OSOCompliance.MissingOSOs.Count > 0)
                sb.AppendLine($"<li><b>Missing OSOs:</b> {string.Join(", ", comp.OSOCompliance.MissingOSOs)}</li>");
            if (comp.OSOCompliance.InsufficientRobustness.Count > 0)
                sb.AppendLine($"<li><b>Insufficient Robustness:</b> {string.Join(", ", comp.OSOCompliance.InsufficientRobustness)}</li>");
            if (comp.OSOCompliance.Warnings.Count > 0)
                sb.AppendLine($"<li><b>Warnings:</b> {string.Join(" | ", comp.OSOCompliance.Warnings)}</li>");
        }
        sb.AppendLine("</ul>");

        sb.AppendLine("<h2>Σημειώσεις</h2>");
        sb.AppendLine($"<div>{System.Web.HttpUtility.HtmlEncode(comp.Notes ?? "")}</div>");

        sb.AppendLine("<h2>Παραπομπές SORA</h2>");
        sb.AppendLine("<ul>");
        sb.AppendLine("<li>JARUS SORA 2.0 / 2.5 — SAIL matrix (Final GRC × Residual ARC → SAIL)</li>");
        sb.AppendLine("<li>OSO requirements per SAIL (Annexes / Tables)</li>");
        sb.AppendLine("</ul>");

        sb.AppendLine("</body></html>");
        return sb.ToString();
    }

    private static byte[] BuildPdf(SAILComplianceResult comp, SAILReportRequest req, DateTime ts, string exposure)
    {
        var title = $"SAIL Compliance Report — SORA {req.SoraVersion}";
        var pdf = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.Size(PageSizes.A4);
                
                // QuestPDF 2024.6+ has automatic fallback for Greek/international characters
                // No manual configuration needed - system fonts automatically support Unicode
                
                page.Header().Row(row =>
                {
                    row.RelativeItem().Text(title).SemiBold().FontSize(16);
                    row.ConstantItem(120).AlignRight().Text($"UTC {ts:yyyy-MM-dd HH:mm}");
                });
                page.Content().Column(column =>
                {
                    column.Spacing(8);
                    if (!string.IsNullOrWhiteSpace(req.MissionName))
                        column.Item().Text($"Mission: {req.MissionName}");
                    if (!string.IsNullOrWhiteSpace(req.MissionId))
                        column.Item().Text($"Mission ID: {req.MissionId}");

                    column.Item().Text("Αποτελέσματα").SemiBold();
                    column.Item().Table(t =>
                    {
                        t.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(2);
                            c.RelativeColumn(5);
                        });
                        void RowKV(string k, string? v)
                        {
                            t.Cell().Element(CellKey).Text(k);
                            t.Cell().Element(CellVal).Text(v ?? "-");
                        }
                        RowKV("Final GRC", req.FinalGRC.ToString());
                        RowKV("Residual ARC", req.ResidualARC.ToString());
                        RowKV("SAIL", comp.SAIL.ToString());
                        RowKV("TMPR", comp.TMPR != null ? $"{comp.TMPR.Level} ({comp.TMPR.Robustness})" : "-");
                        RowKV("Compliance", comp.IsCompliant ? "COMPLIANT" : "NON-COMPLIANT");
                        RowKV("Exposure", exposure);
                    });

                    column.Item().Text("OSO Συμμόρφωση").SemiBold();
                    column.Item().Column(list =>
                    {
                        if (comp.OSOCompliance != null)
                        {
                            if (comp.OSOCompliance.MissingOSOs.Count > 0)
                                list.Item().Text($"Missing OSOs: {string.Join(", ", comp.OSOCompliance.MissingOSOs)}");
                            if (comp.OSOCompliance.InsufficientRobustness.Count > 0)
                                list.Item().Text($"Insufficient Robustness: {string.Join(", ", comp.OSOCompliance.InsufficientRobustness)}");
                            if (comp.OSOCompliance.Warnings.Count > 0)
                                list.Item().Text($"Warnings: {string.Join(" | ", comp.OSOCompliance.Warnings)}");
                        }
                    });

                    column.Item().Text("Σημειώσεις").SemiBold();
                    column.Item().Text(comp.Notes ?? "");

                    column.Item().Text("Παραπομπές SORA").SemiBold();
                    column.Item().Text("JARUS SORA 2.0 / 2.5 — SAIL matrix, OSO per SAIL");
                });
                page.Footer().AlignRight().Text(x =>
                {
                    x.Span("Skyworks SAIL Doc ");
                    x.Span(DateTime.UtcNow.ToString("yyyy"));
                });
            });
        }).GeneratePdf();

        return pdf;

        IContainer CellKey(IContainer x) => x.PaddingVertical(2).PaddingRight(6).Background(Colors.Grey.Lighten3);
        IContainer CellVal(IContainer x) => x.PaddingVertical(2).PaddingLeft(6);
    }
}
