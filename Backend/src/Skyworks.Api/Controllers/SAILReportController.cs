using Microsoft.AspNetCore.Mvc;
using Skyworks.Core.Services.Reports;
using Microsoft.Extensions.Localization;

namespace Skyworks.Api.Controllers;

[ApiController]
[Route("api/sail/report")]
public class SAILReportController : ControllerBase
{
    private readonly ISAILReportService _report;
    private readonly IWebHostEnvironment _env;
    private readonly IStringLocalizer<Resources.SharedResources> _localizer;

    public SAILReportController(
        ISAILReportService report,
        IWebHostEnvironment env,
        IStringLocalizer<Resources.SharedResources> localizer)
    {
        _report = report;
        _env = env;
        _localizer = localizer;
    }

    [HttpPost]
    public IActionResult Generate([FromBody] SAILReportRequest request, [FromQuery] string format = "pdf", [FromQuery] bool save = false)
    {
        var result = _report.GenerateReport(request);
        if (save)
        {
            try
            {
                // Determine workspace root (Backend/src/Skyworks.Api -> repo root is 3 levels up)
                var contentRoot = _env.ContentRootPath;
                var workspaceRoot = Path.GetFullPath(Path.Combine(contentRoot, "..", "..", ".."));
                var deliverDir = Path.Combine(workspaceRoot, "Docs", "Compliance", "Deliverables");
                Directory.CreateDirectory(deliverDir);

                if (string.Equals(format, "html", StringComparison.OrdinalIgnoreCase))
                {
                    var htmlPath = Path.Combine(deliverDir, Path.ChangeExtension(result.FileName, ".html"));
                    System.IO.File.WriteAllText(htmlPath, result.Html);
                }
                else
                {
                    var pdfPath = Path.Combine(deliverDir, result.FileName);
                    System.IO.File.WriteAllBytes(pdfPath, result.Pdf);
                }
            }
            catch
            {
                // best-effort save; continue returning the file
            }
        }
        if (string.Equals(format, "html", StringComparison.OrdinalIgnoreCase))
        {
            return Content(result.Html, "text/html; charset=utf-8");
        }
        return File(result.Pdf, "application/pdf", result.FileName);
    }
}
