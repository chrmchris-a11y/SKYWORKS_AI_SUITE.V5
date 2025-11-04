# Dual SORA Implementation Guide

**Version:** 1.0  
**Last Updated:** 2025-01-27  
**SORA Support:** EASA AMC UAS.SORA-10 (2.0) + JARUS SORA 2.5  
**Audience:** Backend Developers  

## Overview

This guide provides practical patterns for implementing dual SORA version support across Skyworks services. Both SORA 2.0 (24 OSOs) and SORA 2.5 (17 OSOs) must be supported simultaneously.

## Version Detection Pattern

### Standard Parameter Convention

```csharp
// ✅ CORRECT: Consistent parameter naming
public async Task<ValidationResult> ValidateOSO(
    string osoId, 
    string soraVersion,  // "2.0" or "2.5"
    Operation operation
)

// ❌ INCORRECT: Inconsistent naming
public async Task<ValidationResult> ValidateOSO(
    string osoId, 
    string version,      // Unclear what version
    Operation operation
)
```

### Version Validation Pattern

```csharp
public class SoraVersionValidator
{
    public static bool IsValidVersion(string soraVersion)
    {
        return soraVersion == "2.0" || soraVersion == "2.5";
    }
    
    public static void ValidateVersion(string soraVersion)
    {
        if (!IsValidVersion(soraVersion))
        {
            throw new ArgumentException(
                $"Unsupported SORA version: {soraVersion}. " +
                $"Supported versions: 2.0, 2.5",
                nameof(soraVersion)
            );
        }
    }
}
```

### Switch-Based Version Logic

Based on TMPRService implementation:

```csharp
public async Task<TMPRValidationResult> ValidateTMPR(
    Operation operation, 
    string osoId, 
    string soraVersion)
{
    // Validate version first
    SoraVersionValidator.ValidateVersion(soraVersion);
    
    // Version-specific logic
    var result = soraVersion switch
    {
        "2.0" => await ValidateTMPRForSora20(operation, osoId),
        "2.5" => await ValidateTMPRForSora25(operation, osoId),
        _ => throw new ArgumentException(
            $"Unsupported SORA version: {soraVersion}", 
            nameof(soraVersion)
        )
    };
    
    return result;
}
```

## API Design Best Practices

### 1. Consistent Service Interfaces

```csharp
// Standard interface pattern for dual SORA support
public interface IOSOService
{
    // Always include soraVersion as second parameter
    Task<OSOValidationResult> ValidateOSO(
        string osoId, 
        string soraVersion, 
        Operation operation
    );
    
    // Version-aware OSO listing
    List<string> GetApplicableOSOs(string soraVersion);
    
    // Version-specific requirements
    Dictionary<string, object> GetOSORequirements(
        string osoId, 
        string soraVersion
    );
}
```

### 2. Controller Parameter Binding

```csharp
[ApiController]
[Route("api/[controller]")]
public class OSOController : ControllerBase
{
    [HttpPost("validate")]
    public async Task<ActionResult<OSOValidationResult>> ValidateOSO(
        [FromBody] OSOValidationRequest request)
    {
        // Validate SORA version in request
        if (!SoraVersionValidator.IsValidVersion(request.SoraVersion))
        {
            return BadRequest($"Invalid SORA version: {request.SoraVersion}");
        }
        
        var result = await _osoService.ValidateOSO(
            request.OsoId,
            request.SoraVersion,
            request.Operation
        );
        
        return Ok(result);
    }
}

public class OSOValidationRequest
{
    public string OsoId { get; set; } = string.Empty;
    public string SoraVersion { get; set; } = "2.5"; // Default to latest
    public Operation Operation { get; set; } = new();
}
```

### 3. Response Model Consistency

```csharp
public class BaseValidationResult
{
    public string SoraVersion { get; set; } = string.Empty;
    public bool IsValid { get; set; }
    public string ValidationMessage { get; set; } = string.Empty;
    public DateTime ValidatedAt { get; set; } = DateTime.UtcNow;
}

public class TMPRValidationResult : BaseValidationResult
{
    public string OsoId { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
}
```

## Real Implementation Examples

### Example 1: TMPRService Version Logic

From `Backend/Services/TMPRService.cs`:

```csharp
// SORA 2.0: 24 OSOs, strict TMPR requirements
private async Task<TMPRValidationResult> ValidateTMPRForSora20(
    Operation operation, 
    string osoId)
{
    // More OSOs require TMPR in SORA 2.0
    string[] tmprRequiredOsos20 = {
        "OSO-10", "OSO-11", "OSO-12", 
        "OSO-14", "OSO-15", "OSO-21", "OSO-22"
    };
    
    // Higher reliability threshold for SORA 2.0
    if (tmprSystems.Any(t => t.ReliabilityFactor >= 0.9))
    {
        result.IsValid = true;
    }
    
    return result;
}

// SORA 2.5: 17 OSOs, focused TMPR requirements
private async Task<TMPRValidationResult> ValidateTMPRForSora25(
    Operation operation, 
    string osoId)
{
    // Fewer OSOs require TMPR in SORA 2.5
    string[] tmprRequiredOsos25 = {
        "OSO-11", "OSO-17", "OSO-23"
    };
    
    // Even higher reliability threshold for SORA 2.5
    if (tmprSystems.Any(t => t.ReliabilityFactor >= 0.95))
    {
        result.IsValid = true;
    }
    
    return result;
}
```

### Example 2: ValidationService ARC Logic

```csharp
public class ValidationService
{
    public async Task<OSOComplianceResult> ValidateOSOCompliance(
        Operation operation, 
        string arc, 
        int sail,
        string soraVersion = "2.5") // Default to latest
    {
        SoraVersionValidator.ValidateVersion(soraVersion);
        
        var applicableOSOs = GetApplicableOSOs(soraVersion);
        
        foreach (var osoId in applicableOSOs)
        {
            var osoResult = soraVersion switch
            {
                "2.0" => await ValidateOSOForSora20(osoId, operation),
                "2.5" => await ValidateOSOForSora25(osoId, operation),
                _ => throw new InvalidOperationException($"Unknown SORA version: {soraVersion}")
            };
            
            result.OSOResults.Add(osoResult);
        }
        
        return result;
    }
    
    private List<string> GetApplicableOSOs(string soraVersion)
    {
        return soraVersion switch
        {
            "2.0" => new List<string> 
            { 
                "OSO-01", "OSO-02", "OSO-03", "OSO-04", "OSO-05",
                "OSO-06", "OSO-07", "OSO-08", "OSO-09", "OSO-10",
                "OSO-11", "OSO-12", "OSO-13", "OSO-14", "OSO-15",
                "OSO-16", "OSO-17", "OSO-18", "OSO-19", "OSO-20",
                "OSO-21", "OSO-22", "OSO-23", "OSO-24"  // 24 OSOs
            },
            "2.5" => new List<string> 
            { 
                "OSO-01", "OSO-02", "OSO-03", "OSO-04", "OSO-05",
                "OSO-06", "OSO-07", "OSO-08", "OSO-09", "OSO-10",
                "OSO-11", "OSO-12", "OSO-13", "OSO-14", "OSO-15",
                "OSO-16", "OSO-17"  // 17 OSOs
            },
            _ => throw new ArgumentException($"Unknown SORA version: {soraVersion}")
        };
    }
}
```

## Migration Guide

### Step 1: Add Version Parameter

**Before (Single Version):**
```csharp
public async Task<Result> ValidateOSO(string osoId, Operation operation)
{
    // SORA 2.5 only logic
    return await ValidateForSora25(osoId, operation);
}
```

**After (Dual Version):**
```csharp
public async Task<Result> ValidateOSO(
    string osoId, 
    string soraVersion, 
    Operation operation)
{
    SoraVersionValidator.ValidateVersion(soraVersion);
    
    return soraVersion switch
    {
        "2.0" => await ValidateForSora20(osoId, operation),
        "2.5" => await ValidateForSora25(osoId, operation),
        _ => throw new ArgumentException($"Unsupported version: {soraVersion}")
    };
}
```

### Step 2: Extract Version-Specific Logic

```csharp
// Create separate methods for each version
private async Task<Result> ValidateForSora20(string osoId, Operation operation)
{
    // SORA 2.0 specific logic (24 OSOs)
    // More comprehensive requirements
    // Greek terms for DCA Cyprus: Λειτουργικός Στόχος Ασφαλείας
}

private async Task<Result> ValidateForSora25(string osoId, Operation operation)
{
    // SORA 2.5 specific logic (17 OSOs)  
    // Streamlined requirements
}
```

### Step 3: Update Interface Contracts

```csharp
// Update all service interfaces
public interface IMyService
{
    Task<Result> ValidateOSO(
        string osoId, 
        string soraVersion,  // Add this parameter
        Operation operation
    );
}
```

### Step 4: Update Controllers

```csharp
[HttpPost("validate")]
public async Task<ActionResult> ValidateOSO(
    [FromBody] ValidationRequest request)
{
    // Add version validation
    if (!SoraVersionValidator.IsValidVersion(request.SoraVersion))
    {
        return BadRequest($"Invalid SORA version: {request.SoraVersion}");
    }
    
    var result = await _service.ValidateOSO(
        request.OsoId,
        request.SoraVersion,  // Pass version parameter
        request.Operation
    );
    
    return Ok(result);
}
```

### Step 5: Update Frontend Integration

```javascript
// Update API calls to include version
async function validateOSO(osoId, operation, soraVersion = '2.5') {
    const response = await fetch('/api/oso/validate', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
            osoId,
            soraVersion,  // Include version in payload
            operation
        })
    });
    return response.json();
}
```

## Testing Strategy

### Unit Test Pattern

```csharp
[Test]
public async Task ValidateOSO_ShouldHandleBothVersions()
{
    // Arrange
    var operation = CreateTestOperation();
    
    // Act & Assert for SORA 2.0
    var result20 = await _service.ValidateOSO("OSO-11", "2.0", operation);
    Assert.That(result20.SoraVersion, Is.EqualTo("2.0"));
    
    // Act & Assert for SORA 2.5
    var result25 = await _service.ValidateOSO("OSO-11", "2.5", operation);
    Assert.That(result25.SoraVersion, Is.EqualTo("2.5"));
}

[Test]
public void ValidateOSO_ShouldRejectInvalidVersion()
{
    // Arrange
    var operation = CreateTestOperation();
    
    // Act & Assert
    Assert.ThrowsAsync<ArgumentException>(
        () => _service.ValidateOSO("OSO-11", "3.0", operation)
    );
}
```

### Integration Test Pattern

```csharp
[Test]
public async Task E2E_ValidationWorkflow_BothVersions()
{
    // Test complete workflow for both versions
    var versions = new[] { "2.0", "2.5" };
    
    foreach (var version in versions)
    {
        var result = await _client.PostAsJsonAsync("/api/oso/validate", 
            new { osoId = "OSO-11", soraVersion = version, operation = _testOperation });
        
        Assert.That(result.IsSuccessStatusCode, Is.True);
        
        var content = await result.Content.ReadFromJsonAsync<ValidationResult>();
        Assert.That(content.SoraVersion, Is.EqualTo(version));
    }
}
```

## Common Patterns & Best Practices

### 1. Default Version Strategy

```csharp
// Always default to the latest version (2.5)
public async Task<Result> ValidateOSO(
    string osoId, 
    Operation operation,
    string soraVersion = "2.5")  // Default parameter
{
    return await ValidateOSO(osoId, soraVersion, operation);
}
```

### 2. Logging Version Context

```csharp
_logger.LogInformation(
    "Validating OSO {OsoId} for SORA {SoraVersion} - Operation: {OperationId}",
    osoId, soraVersion, operation.Id
);
```

### 3. Error Handling

```csharp
try
{
    var result = soraVersion switch
    {
        "2.0" => await ValidateForSora20(osoId, operation),
        "2.5" => await ValidateForSora25(osoId, operation),
        _ => throw new ArgumentException($"Unsupported SORA version: {soraVersion}")
    };
    return result;
}
catch (ArgumentException ex)
{
    _logger.LogError(ex, "Invalid SORA version: {SoraVersion}", soraVersion);
    throw;
}
```

### 4. Version-Aware Configuration

```csharp
public class SoraConfiguration
{
    public Dictionary<string, SoraVersionConfig> Versions { get; set; } = new()
    {
        ["2.0"] = new SoraVersionConfig 
        { 
            OSOs = 24, 
            RequiresRobustness = true,
            DefaultReliabilityThreshold = 0.9 
        },
        ["2.5"] = new SoraVersionConfig 
        { 
            OSOs = 17, 
            RequiresRobustness = false,
            DefaultReliabilityThreshold = 0.95 
        }
    };
}
```

## Version Differences Summary

| Feature | SORA 2.0 | SORA 2.5 |
|---------|----------|----------|
| Total OSOs | 24 | 17 |
| TMPR Requirements | More extensive | Focused |
| Reliability Thresholds | 90% | 95% |
| Documentation Depth | Comprehensive | Streamlined |
| Validation Complexity | High | Medium |

## Integration Points

### With MCP Server
- Use `get_sora_context` tool for version-specific rules
- Reference `sora-2.0-tables` and `sora-2.5-tables` resources
- Validate against version-appropriate OSO requirements

### With Frontend
- Pass `soraVersion` in all API requests
- Display version-specific UI elements
- Handle version-specific validation responses

### With Documentation
- Reference version-specific compliance guides
- Link to appropriate SORA documentation
- Maintain version-aware help text

---

**Note:** This guide focuses on backend implementation patterns. For complete examples, refer to `TMPRService.cs` and `ValidationService.cs` in the codebase. Greek terminology (Λειτουργικός Στόχος Ασφαλείας) should be used where required for DCA Cyprus compliance.