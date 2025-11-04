# Developer Guide - Skyworks AI Suite V5

**Version:** 1.0  
**Last Updated:** 2025-01-27  
**Target:** New developers joining the project  

## Project Overview

**Skyworks AI Suite V5** is a comprehensive UAS (Unmanned Aircraft Systems) risk management platform that ensures compliance with both **SORA 2.0 (EASA AMC)** and **JARUS SORA 2.5** regulations.

### What We Build
- **Risk Assessment Engine:** Calculates GRC, ARC, SAIL levels
- **OSO Validation System:** Validates Operational Safety Objectives 
- **TMPR Management:** Tactical Mitigation Performance Requirements
- **Compliance Documentation:** Automated evidence collection
- **Dual SORA Support:** Both 2.0 (24 OSOs) and 2.5 (17 OSOs)

### Key Business Logic
```
iGRC → Final GRC → Initial ARC → Residual ARC → SAIL → OSO → TMPR
```

## Architecture Overview

### Backend (.NET 8 C#)
- **Core Services:** Risk calculation, OSO validation, TMPR management
- **API Layer:** RESTful endpoints with dual SORA version support
- **Models:** Domain objects supporting both SORA versions
- **Controllers:** Version-aware request handling

### Frontend (Vanilla JS + HTML)
- **Pages:** Interactive risk assessment interfaces
- **Components:** Reusable UI elements with SORA version toggles
- **Internationalization:** English/Greek support (DCA Cyprus compliance)

### MCP Server (Knowledge Base)
- **SORA Tables:** Pre-loaded reference tables for both versions
- **Rules Engine:** Authoritative SORA methodology implementation
- **Context Provider:** Real-time regulatory guidance

## Project Structure

### Backend Organization
```
Backend/
├── src/
│   ├── Skyworks.Api/          # Web API controllers
│   ├── Skyworks.Core/         # Business logic & models
│   ├── Skyworks.Infrastructure/ # Data access & external services
│   └── Skyworks.AgentComm/    # MCP server communication
├── Services/                   # Legacy service implementations
│   ├── OSOService.cs          # OSO validation logic
│   ├── TMPRService.cs         # TMPR validation (dual version)
│   ├── ValidationService.cs   # Floor Rule + M3 penalties
│   └── RiskMitigationService.cs # Risk mitigation strategies
├── Controllers/               # API controllers
│   ├── OSOController.cs       # OSO management endpoints
│   ├── TMPRController.cs      # TMPR validation endpoints
│   └── ValidationController.cs # Compliance validation
├── Models/                    # Domain models
├── tests/                     # Unit & integration tests
└── Skyworks.sln              # Solution file
```

### Frontend Organization  
```
Frontend/
├── Pages/                     # Main application pages
│   ├── oso-manager.js         # OSO management interface
│   └── oso-ui-v2.js          # Updated OSO interface with dual SORA
├── Components/               # Reusable UI components
│   └── oso-references.html   # OSO reference materials
├── assets/                   # Static assets (CSS, images)
└── i18n/                     # Internationalization files
```

### Documentation Structure
```
Docs/
├── Compliance/               # SORA compliance documentation
│   ├── OSO_BEST_PRACTICES.md # OSO implementation guidelines
│   ├── OSO_DETAILED_RULES.md # Detailed OSO specifications  
│   └── OSO_CHECKLIST.md     # Compliance verification checklist
├── API/                      # API documentation
│   ├── DUAL_SORA_IMPLEMENTATION_GUIDE.md # This file you're reading
│   ├── OSO_IMPLEMENTATION_GUIDE.md # OSO API patterns
│   └── OSO_COMMENTS_SYSTEM.md # Evidence tracking system
└── Knowledge/                # Project knowledge base
    ├── PROJECT_PHASES_12.md  # Overall project roadmap
    └── PROJECT_STATUS.json   # Machine-readable status
```

## Development Workflow

### Getting Started
1. **Clone Repository**
   ```bash
   git clone <repository-url>
   cd skyworks-ai-suite
   ```

2. **Backend Setup**
   ```bash
   cd Backend
   dotnet restore
   dotnet build
   dotnet test
   ```

3. **Frontend Setup**
   ```bash
   cd Frontend  
   # Open index.html in browser or use live server
   ```

4. **MCP Server** (automatically loaded)
   - Pre-configured with 23 EASA/JARUS documents
   - Available tools: `get_sora_context`, `calculate_sail`, etc.

### Git Workflow
- **Main Branch:** Production-ready code
- **Feature Branches:** `feature/step-XX-description`  
- **Pull Requests:** Required, include tests
- **Dual Version Lint:** CI blocks merges without both SORA version support

### Build & Test
```bash
# Backend build
dotnet build Backend/Skyworks.sln

# Run all tests (256+ tests)
dotnet test Backend/tests/

# Specific test categories
dotnet test --filter "Category=SORA20"
dotnet test --filter "Category=SORA25"
```

## Common Development Tasks

### Task 1: Add New OSO Validation

#### Step 1: Update Models
```csharp
// Backend/Models/OSORequirement.cs
public class OSORequirement
{
    public string OSOId { get; set; } = string.Empty;
    public string SoraVersion { get; set; } = string.Empty; // "2.0" or "2.5"
    public List<string> RequiredEvidence { get; set; } = new();
}
```

#### Step 2: Implement Service Logic
```csharp
// Backend/Services/OSOService.cs
public async Task<OSOValidationResult> ValidateOSO(
    string osoId, 
    string soraVersion, 
    Operation operation)
{
    // Always validate version first
    SoraVersionValidator.ValidateVersion(soraVersion);
    
    return soraVersion switch
    {
        "2.0" => await ValidateOSOForSora20(osoId, operation),
        "2.5" => await ValidateOSOForSora25(osoId, operation),
        _ => throw new ArgumentException($"Unknown SORA version: {soraVersion}")
    };
}
```

#### Step 3: Add Controller Endpoint
```csharp
// Backend/Controllers/OSOController.cs
[HttpPost("validate")]
public async Task<ActionResult<OSOValidationResult>> ValidateOSO(
    [FromBody] OSOValidationRequest request)
{
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
```

#### Step 4: Add Tests
```csharp
[Test]
public async Task ValidateOSO_ShouldSupportBothVersions()
{
    // Test SORA 2.0
    var result20 = await _service.ValidateOSO("OSO-XX", "2.0", _testOperation);
    Assert.That(result20.SoraVersion, Is.EqualTo("2.0"));
    
    // Test SORA 2.5  
    var result25 = await _service.ValidateOSO("OSO-XX", "2.5", _testOperation);
    Assert.That(result25.SoraVersion, Is.EqualTo("2.5"));
}
```

### Task 2: Add New Service with Dual SORA Support

#### Service Interface Pattern
```csharp
public interface IMyNewService
{
    Task<MyResult> ProcessData(
        string dataId,
        string soraVersion,  // Always second parameter
        MyData data
    );
    
    List<string> GetApplicableRules(string soraVersion);
}
```

#### Implementation Pattern
```csharp
public class MyNewService : IMyNewService
{
    public async Task<MyResult> ProcessData(
        string dataId, 
        string soraVersion, 
        MyData data)
    {
        _logger.LogInformation(
            "Processing {DataId} for SORA {SoraVersion}", 
            dataId, soraVersion
        );
        
        SoraVersionValidator.ValidateVersion(soraVersion);
        
        return soraVersion switch
        {
            "2.0" => await ProcessForSora20(dataId, data),
            "2.5" => await ProcessForSora25(dataId, data), 
            _ => throw new ArgumentException($"Unsupported version: {soraVersion}")
        };
    }
}
```

### Task 3: Update SORA Version Requirements

#### Version-Specific Constants
```csharp
public static class SoraVersionConstants
{
    public static class Sora20
    {
        public const int TotalOSOs = 24;
        public const double TMPRReliabilityThreshold = 0.90;
        public static readonly string[] TMPRRequiredOSOs = 
            { "OSO-10", "OSO-11", "OSO-12", "OSO-14", "OSO-15", "OSO-21", "OSO-22" };
    }
    
    public static class Sora25  
    {
        public const int TotalOSOs = 17;
        public const double TMPRReliabilityThreshold = 0.95;
        public static readonly string[] TMPRRequiredOSOs = 
            { "OSO-11", "OSO-17", "OSO-23" };
    }
}
```

## Testing Strategy

### Unit Tests
- **Location:** `Backend/tests/`
- **Pattern:** One test class per service
- **Coverage:** Both SORA versions for every method

### Integration Tests
- **API Tests:** Full request/response validation
- **Database Tests:** Data persistence verification  
- **MCP Integration:** Knowledge base connectivity

### Required Test Categories
```csharp
[Test]
[Category("SORA20")]
public void TestSora20HappyPath() { }

[Test]  
[Category("SORA25")]
public void TestSora25HappyPath() { }

[Test]
[Category("M3Penalty")]
public void TestMaxM3PenaltiesBothVersions() { }

[Test]
[Category("TMPR")]
public void TestTMPRBoundaryBothVersions() { }
```

## Troubleshooting

### Common Issues

#### 1. "Invalid SORA version" Errors
**Problem:** API requests failing with version validation errors
**Solution:** 
```csharp
// Always validate version in requests
if (!SoraVersionValidator.IsValidVersion(request.SoraVersion))
{
    return BadRequest($"Invalid SORA version: {request.SoraVersion}. Supported: 2.0, 2.5");
}
```

#### 2. Missing OSO for Version
**Problem:** OSO exists in one version but not the other
**Solution:**
```csharp
// Check OSO exists for version before processing
var applicableOSOs = GetOSOsForVersion(soraVersion);
if (!applicableOSOs.Contains(osoId))
{
    throw new InvalidOperationException($"OSO {osoId} not applicable for SORA {soraVersion}");
}
```

#### 3. Test Failures on Version Switch
**Problem:** Tests pass for 2.5 but fail for 2.0
**Solution:**
- Check version-specific constants (24 vs 17 OSOs)
- Verify TMPR thresholds (0.90 vs 0.95)
- Ensure separate validation logic

#### 4. MCP Server Connection Issues
**Problem:** Knowledge base queries failing
**Solution:**
```bash
# Check MCP server status
curl http://localhost:3001/health

# Restart if needed
npm run start:mcp
```

### Debugging Tips

#### Enable Detailed Logging
```csharp
_logger.LogDebug("Validating OSO {OsoId} for SORA {SoraVersion} with ARC {ARC}", 
    osoId, soraVersion, arc);
```

#### Test Version-Specific Logic
```csharp
// Test both versions in same test
var versions = new[] { "2.0", "2.5" };
foreach (var version in versions)
{
    var result = await _service.Process(data, version);
    Assert.That(result.SoraVersion, Is.EqualTo(version));
}
```

#### Validate API Responses
```bash
# Check API status endpoint
curl http://localhost:5000/api/status

# Expected response includes both versions
{
  "soraVersions": ["2.0", "2.5"],
  "currentStep": 49,
  "build": "SUCCESS"
}
```

## Key Development Rules

### ✅ Always Do
- Include `soraVersion` parameter in all OSO/TMPR methods
- Validate SORA version before processing
- Log version context in all operations  
- Test both versions for every new feature
- Document version differences in code comments

### ❌ Never Do
- Mix SORA 2.0 and 2.5 logic in same method
- Assume default version without validation
- Skip version-specific tests
- Use hard-coded OSO counts
- Deploy without dual version support

### 🔧 Before Each Commit
- [ ] All tests pass for both SORA versions
- [ ] New services include version parameter
- [ ] API endpoints validate version input
- [ ] Documentation updated with version info
- [ ] CI dual-version lint checks pass

## Resources

### Internal Documentation
- `OSO_BEST_PRACTICES.md` - OSO implementation guidelines
- `DUAL_SORA_IMPLEMENTATION_GUIDE.md` - Technical patterns
- `PROJECT_STATUS.md` - Current project status

### External References
- EASA AMC/GM to UAS.SORA-10 (SORA 2.0)
- JARUS SORA 2.5 Main Body + Annexes A-D
- DCA Cyprus UAS Regulations (Greek terminology)

### Development Tools
- **IDE:** Visual Studio 2022 or VS Code
- **Testing:** NUnit framework
- **API Testing:** Postman collections available
- **Documentation:** Markdown with GitHub rendering

---

**Welcome to the team!** 🚁 For questions, check existing code patterns in `TMPRService.cs` and `ValidationService.cs`, or reach out to the development team.