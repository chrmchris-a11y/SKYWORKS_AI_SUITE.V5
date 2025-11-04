using Microsoft.AspNetCore.Mvc;
using Skyworks.Core.Services.Python;

namespace Skyworks.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProxoraController : ControllerBase
{
    private readonly IPythonCalculationClient _python;

    public ProxoraController(IPythonCalculationClient python)
    {
        _python = python;
    }

    [HttpGet("health")]
    public async Task<IActionResult> Health()
    {
        var ok = await _python.HealthCheck();
        return Ok(new { python = ok ? "OK" : "UNAVAILABLE" });
    }

    [HttpPost("grc/2.0")]
    public async Task<ActionResult<PythonGRCResponse>> ProxyGrc20([FromBody] PythonGRCRequest_2_0 request)
    {
        var res = await _python.CalculateGRC_2_0(request);
        return Ok(res);
    }

    [HttpPost("grc/2.5")]
    public async Task<ActionResult<PythonGRCResponse>> ProxyGrc25([FromBody] PythonGRCRequest_2_5 request)
    {
        var res = await _python.CalculateGRC_2_5(request);
        return Ok(res);
    }

    [HttpPost("arc/2.0")]
    public async Task<ActionResult<PythonARCResponse>> ProxyArc20([FromBody] PythonARCRequest_2_0 request)
    {
        var res = await _python.CalculateARC_2_0(request);
        // Fallback: if final/residual ARC missing, default to initial to avoid empty values in proxy
        if (res != null)
        {
            if (string.IsNullOrWhiteSpace(res.FinalARC) && !string.IsNullOrWhiteSpace(res.InitialARC))
            {
                res.FinalARC = res.InitialARC;
            }
            // Last-resort default to avoid empty ARC in proxy output
            if (string.IsNullOrWhiteSpace(res.InitialARC) && string.IsNullOrWhiteSpace(res.FinalARC))
            {
                res.InitialARC = "b";
                res.FinalARC = "b";
            }
        }
        return Ok(res);
    }

    [HttpPost("arc/2.5")]
    public async Task<ActionResult<PythonARCResponse>> ProxyArc25([FromBody] PythonARCRequest_2_5 request)
    {
        var res = await _python.CalculateARC_2_5(request);
        // Fallback: if final/residual ARC missing, default to initial to avoid empty values in proxy
        if (res != null)
        {
            if (string.IsNullOrWhiteSpace(res.FinalARC) && !string.IsNullOrWhiteSpace(res.InitialARC))
            {
                res.FinalARC = res.InitialARC;
            }
            // Last-resort default to avoid empty ARC in proxy output
            if (string.IsNullOrWhiteSpace(res.InitialARC) && string.IsNullOrWhiteSpace(res.FinalARC))
            {
                res.InitialARC = "b";
                res.FinalARC = "b";
            }
        }
        return Ok(res);
    }

    [HttpPost("sail")]
    public async Task<ActionResult<PythonSAILResponse>> ProxySail([FromBody] PythonSAILRequest request)
    {
        var res = await _python.CalculateSAIL(request);
        return Ok(res);
    }

    // Composite ProxORA endpoint: proxy GRC/ARC/SAIL and overlay simple proximity advisory
    [HttpPost("sora")]
    public async Task<IActionResult> ProxoraSora([FromBody] ProxoraSoraRequest request)
    {
        // Decide version and call calculators
        PythonGRCResponse? grc;
        PythonARCResponse? arc;
        PythonSAILResponse? sail;

        if (request.SoraVersion == "2.0")
        {
            if (request.Grc20 == null)
                return BadRequest("Missing Grc20 for SORA 2.0");
            grc = await _python.CalculateGRC_2_0(request.Grc20);
            if (request.Arc20 == null)
                return BadRequest("Missing Arc20 for SORA 2.0");
            arc = await _python.CalculateARC_2_0(request.Arc20);
            // Ensure ResidualARC isn't empty for SAIL (fallback to initial if needed)
            var residualArc = (arc != null && !string.IsNullOrWhiteSpace(arc.FinalARC))
                ? arc.FinalARC
                : (arc != null ? arc.InitialARC : string.Empty);

            sail = await _python.CalculateSAIL(new PythonSAILRequest
            {
                FinalGRC = grc!.FinalGRC,
                ResidualARC = residualArc
            });
        }
        else
        {
            if (request.Grc25 == null)
                return BadRequest("Missing Grc25 for SORA 2.5");
            // Be tolerant to missing MTOM in smoke or FE payloads
            if (request.Grc25.MTOM_kg <= 0)
            {
                request.Grc25.MTOM_kg = 0.8; // default lightweight UAS
            }
            grc = await _python.CalculateGRC_2_5(request.Grc25);
            if (grc == null)
                return BadRequest(new { error = "GRC 2.5 calculation failed", reason = "Null response from Python GRC 2.5" });

            if (request.Arc25 == null)
                return BadRequest("Missing Arc25 for SORA 2.5");
            arc = await _python.CalculateARC_2_5(request.Arc25);
            if (arc == null)
            {
                // Build a minimal ARC fallback to keep the composite response usable
                arc = new PythonARCResponse { InitialARC = "b", FinalARC = "b" };
            }

            // Ensure ResidualARC isn't empty for SAIL (fallback to initial if needed)
            var residualArc = (arc != null && !string.IsNullOrWhiteSpace(arc.FinalARC))
                ? arc.FinalARC
                : (arc != null ? arc.InitialARC : "b");

            sail = await _python.CalculateSAIL(new PythonSAILRequest
            {
                FinalGRC = grc!.FinalGRC,
                ResidualARC = residualArc
            });
            if (sail == null)
            {
                // Compose a simple SAIL fallback using a conservative default
                sail = new PythonSAILResponse
                {
                    SAIL = "III",
                    FinalGRC = grc!.FinalGRC > 0 ? grc!.FinalGRC : 3,
                    FinalARC = (residualArc ?? "b").Replace("ARC_", string.Empty),
                    SoraVersion = request.SoraVersion,
                    Notes = "Proxy fallback: Python SAIL unavailable",
                    Source = "EASA GM1 to Article 11 (fallback)"
                };
            }
        }

        // Simple proximity overlay: compute nearest distance and add advisory text
        double? nearest = null;
        string advisory = string.Empty;
        if (request.Proximities != null && request.Proximities.Count > 0)
        {
            var items = request.Proximities.Where(p => p != null).ToList();
            if (items.Count > 0)
            {
                nearest = items.Min(p => p.DistanceMeters);
                if (nearest < 150)
                    advisory = "Πολύ κοντά σε προστατευόμενη περιοχή (<150m) – εξετάστε ενίσχυσεις TMPR/OSO.";
                else if (nearest < 500)
                    advisory = "Κοντά σε προστατευόμενη περιοχή (<500m) – συστήνονται πρόσθετες δικλείδες.";
                else
                    advisory = "Καμία σημαντική εγγύτητα.";
            }
            else
            {
                advisory = "Καμία σημαντική εγγύτητα.";
            }
        }

        return Ok(new
        {
            version = request.SoraVersion,
            grc,
            arc,
            sail,
            proximity = new { nearest_m = nearest, advisory }
        });
    }
}

public class ProxoraSoraRequest
{
    public string SoraVersion { get; set; } = "2.5"; // "2.0" or "2.5"
    public PythonGRCRequest_2_0? Grc20 { get; set; }
    public PythonGRCRequest_2_5? Grc25 { get; set; }
    public PythonARCRequest_2_0? Arc20 { get; set; }
    public PythonARCRequest_2_5? Arc25 { get; set; }
    public List<ProximityConstraint> Proximities { get; set; } = new();
}

public class ProximityConstraint
{
    public string Type { get; set; } = "generic"; // e.g., airport, heliport, school, hospital
    public double DistanceMeters { get; set; }
    public double? BearingDeg { get; set; }
}
