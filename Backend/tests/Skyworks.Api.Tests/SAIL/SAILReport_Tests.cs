using Xunit;
using Skyworks.Core.Services;
using Skyworks.Core.Services.GRC;
using Skyworks.Core.Services.Reports;
using Skyworks.Core.Models.ARC;

namespace Skyworks.Api.Tests.SAIL;

public class SAILReport_Tests
{
    private readonly ISAILReportService _reports;

    public SAILReport_Tests()
    {
        var sail = new SAILService();
        var oso = new OSOService();
        var grc = new GRCCalculationService();
        var compliance = new SAILComplianceService(sail, oso, grc);
        _reports = new SAILReportService(compliance);
    }

    [Fact]
    public void Report_Html_Contains_Key_Fields()
    {
        var req = new SAILReportRequest
        {
            SoraVersion = "2.5",
            FinalGRC = 3,
            ResidualARC = ARCRating.ARC_a,
            MissionId = "TST-001",
            MissionName = "Unit Test Mission"
        };

        var res = _reports.GenerateReport(req);
        Assert.False(string.IsNullOrWhiteSpace(res.Html));
        Assert.Contains("SAIL Compliance Report", res.Html);
        Assert.Contains("Final GRC", res.Html);
        Assert.Contains("Residual ARC", res.Html);
        Assert.Contains("SAIL", res.Html);
        Assert.Contains("TMPR", res.Html);
        Assert.Contains("OSO", res.Html);
    }

    [Fact]
    public void Report_Pdf_Is_Not_Empty()
    {
        var req = new SAILReportRequest
        {
            SoraVersion = "2.5",
            FinalGRC = 4,
            ResidualARC = ARCRating.ARC_a
        };
        var res = _reports.GenerateReport(req);
        Assert.NotNull(res.Pdf);
        Assert.True(res.Pdf.Length > 500); // basic smoke test
    }
}
