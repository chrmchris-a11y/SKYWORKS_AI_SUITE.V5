using Xunit;
using Skyworks.Core.Services.GRC;

namespace Skyworks.Api.Tests.ARC_SAIL;

public class ARC_Mitigations_List_Tests
{
    private readonly IGRCCalculationService _svc = new GRCCalculationService();

    [Fact]
    public void Mitigations_List_Contains_S1_to_S4()
    {
        var list = _svc.GetRecognizedStrategicMitigations();
        Assert.Contains(list, x => x.Id == "S1");
        Assert.Contains(list, x => x.Id == "S2");
        Assert.Contains(list, x => x.Id == "S3");
        Assert.Contains(list, x => x.Id == "S4");
    }
}
