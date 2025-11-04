# PROMPT 1: SORA 2.0 M2 Mitigation Level Correction - Backend Integration

## Executive Summary
This prompt addresses the correction of the **M2 (Impact Reduction) dropdown** in SORA 2.0 from an incorrect "Medium" value to the correct "Low" value per official EASA/JARUS specifications. The frontend (UI) has been corrected, but the backend must now be updated to ensure:
1. GRC calculation engines accept the correct M2Level20 enum values
2. Validation logic rejects "Medium" and accepts only None/Low/High
3. Python FastAPI and .NET Core services properly integrate this fix

## Official JARUS Reference

**Document**: JAR_doc_06 - JARUS SORA 2.0 AMC (Acceptable Means of Compliance)  
**Section**: Table 3 - Ground Risk Mitigations  
**Specification**: M2 mitigation levels for SORA 2.0 are:
- **None (0)**: No impact reduction system
- **Low (-1)**: Basic impact reduction (e.g., simple parachute, foam padding)
- **High (-2)**: Advanced impact reduction (e.g., certified parachute system, dual deployment)

**Critical Finding**: The previous implementation incorrectly used "Medium (-1)" instead of "Low (-1)". SORA 2.0 AMC **does not define a "Medium" level** for M2 mitigation.

**Note**: SORA 2.5 introduces a different M2 scale (None/Medium/High) - this prompt addresses **SORA 2.0 only**.

## Files Changed (Frontend - Already Fixed)

### File: `app/Pages/mission.html`
**Line 889** (corrected):
```html
<label>M2 - Impact reduction (e.g., parachute)
  <select id="m2_20">
    <option value="0">None (0)</option>
    <option value="-1">Low (-1)</option>  <!-- ✅ FIXED: was "Medium" -->
    <option value="-2">High (-2)</option>
  </select>
</label>
```

**JavaScript Data Collection** (line ~2789):
```javascript
const m2_20 = document.getElementById('m2_20')?.value || '0';
```

## Backend Files Requiring Updates

### 1. Python Backend: `Backend_Python/models/grc_models.py`

**Current Code** (lines 25-30):
```python
class M2Level20(str, Enum):
    NONE = "None"
    LOW = "Low"      # ← This is correct
    HIGH = "High"
```

**✅ Status**: Python enum is **already correct** - no changes needed.

**Action Required**: Verify that GRC calculation functions use this enum correctly and do NOT accept "Medium" values.

### 2. .NET Backend: `Backend/src/Skyworks.Core/Models/GrcModels.cs`

**Expected Code** (verify exists):
```csharp
public enum M2Level20
{
    None = 0,
    Low = -1,   // ← Must match Python backend
    High = -2
}
```

**Action Required**: 
- Confirm this enum exists and matches Python
- If "Medium" exists in the enum, **remove it**
- Update any validators that may reference "Medium"

### 3. Python GRC Calculation Engine: `Backend_Python/grc/calculators/grc_calculator_v20.py`

**Critical Logic to Verify**:
```python
def calculate_final_grc_sora_20(initial_grc: int, m1: int, m2: M2Level20, m3: int) -> int:
    """
    Calculate Final GRC for SORA 2.0
    
    Formula: Final GRC = Initial GRC + M1 + M2 + M3
    
    Where:
    - Initial GRC: From iGRC table (Step #2)
    - M1: Mitigation (strategic/tactical) [-1, -2]
    - M2: Impact reduction (M2Level20 enum)
        * None: 0
        * Low: -1     ← Must be exactly this value
        * High: -2
    - M3: Effective deployment (tactical) [-1, -2]
    
    Official Reference: JAR_doc_06 Table 3
    """
    m2_value = 0
    if m2 == M2Level20.LOW:
        m2_value = -1   # ← This must be correct
    elif m2 == M2Level20.HIGH:
        m2_value = -2
    elif m2 == M2Level20.NONE:
        m2_value = 0
    else:
        raise ValueError(f"Invalid M2Level20 value: {m2}. Expected None/Low/High only.")
    
    final_grc = initial_grc + m1 + m2_value + m3
    return max(1, final_grc)  # Final GRC cannot be less than 1
```

**Action Required**:
- Ensure the `m2_value` mapping is exactly as shown above
- Add validation to **reject** any value other than None/Low/High
- Test with both "Low" (-1) and ensure it calculates correctly

### 4. .NET Validation: `Backend/src/Skyworks.Core/Services/GRC/GrcValidatorV20.cs`

**Required Validation Logic**:
```csharp
public class GrcValidatorV20 : IGrcValidator
{
    public ValidationResult ValidateM2(M2Level20 m2Level)
    {
        // Ensure only valid SORA 2.0 M2 levels are accepted
        var validLevels = new[] { M2Level20.None, M2Level20.Low, M2Level20.High };
        
        if (!validLevels.Contains(m2Level))
        {
            return ValidationResult.Fail(
                "Invalid M2 level for SORA 2.0. Expected: None (0), Low (-1), or High (-2). " +
                "Reference: JAR_doc_06 Table 3."
            );
        }
        
        return ValidationResult.Success();
    }
    
    public int GetM2Value(M2Level20 m2Level)
    {
        return m2Level switch
        {
            M2Level20.None => 0,
            M2Level20.Low => -1,   // ← This is the corrected value
            M2Level20.High => -2,
            _ => throw new ArgumentException(
                $"Invalid M2 level: {m2Level}. SORA 2.0 only supports None/Low/High. " +
                "If you need Medium, use SORA 2.5 instead."
            )
        };
    }
}
```

**Action Required**:
- Implement or verify this validator exists
- Ensure error messages reference JAR_doc_06 Table 3
- Add unit tests for all three valid values (None/Low/High)
- Add negative test to ensure "Medium" is rejected with clear error

### 5. API Endpoint: `Backend/src/Skyworks.Api/Controllers/SoraController.cs`

**Expected Behavior**:
```csharp
[HttpPost("complete")]
public async Task<IActionResult> CalculateCompleteSora([FromBody] SoraRequest request)
{
    if (request.Category == "SORA-2.0")
    {
        // Validate M2 level for SORA 2.0
        var validationResult = _grcValidator20.ValidateM2(request.GrcInputs.M2);
        if (!validationResult.IsValid)
        {
            return BadRequest(new { 
                error = "Invalid M2 level", 
                details = validationResult.ErrorMessage,
                reference = "JAR_doc_06 Table 3 - SORA 2.0 AMC"
            });
        }
        
        // Calculate GRC using SORA 2.0 logic
        var finalGrc = await _grcCalculator20.CalculateFinalGrc(
            request.GrcInputs.InitialGrc,
            request.GrcInputs.M1,
            request.GrcInputs.M2,  // ← This will now be None/Low/High only
            request.GrcInputs.M3
        );
        
        // Continue with ARC and SAIL calculations...
    }
}
```

**Action Required**:
- Verify the controller properly validates M2 values
- Ensure the 400 Bad Request error from user testing is fixed
- Return clear error messages referencing official JARUS documents

## Python FastAPI Integration

### File: `Backend_Python/main.py` (Port 8001)

**Critical**: The user reported that Python FastAPI is **NOT running** on port 8001. This is causing GRC calculation failures.

**Required Actions**:
1. **Start Python FastAPI** on port 8001
2. **Verify Health Endpoint**: `http://localhost:8001/health` should return 200 OK
3. **Test GRC Endpoint**: `http://localhost:8001/api/grc/calculate-v20` should accept:
   ```json
   {
     "initial_grc": 5,
     "m1": -1,
     "m2": "Low",   // ← This must work (was "Medium" before)
     "m3": 0
   }
   ```
4. **Expected Response**:
   ```json
   {
     "final_grc": 3,
     "calculation": "5 + (-1) + (-1) + 0 = 3"
   }
   ```

### File: `Backend_Python/routes/grc_routes.py`

**Expected Endpoint**:
```python
@router.post("/api/grc/calculate-v20")
async def calculate_grc_v20(request: GrcRequestV20):
    """
    Calculate Final GRC for SORA 2.0
    
    Official Reference: JAR_doc_06 Table 3
    """
    try:
        # Validate M2 level
        if request.m2 not in ["None", "Low", "High"]:
            raise HTTPException(
                status_code=400,
                detail=f"Invalid M2 level '{request.m2}'. SORA 2.0 only supports None, Low, High. "
                       "Reference: JAR_doc_06 Table 3. If you need Medium, use SORA 2.5."
            )
        
        # Map M2 to numeric value
        m2_value = {"None": 0, "Low": -1, "High": -2}[request.m2]
        
        # Calculate Final GRC
        final_grc = request.initial_grc + request.m1 + m2_value + request.m3
        final_grc = max(1, final_grc)  # Cannot be less than 1
        
        return GrcResponseV20(
            final_grc=final_grc,
            initial_grc=request.initial_grc,
            m1=request.m1,
            m2=request.m2,
            m2_value=m2_value,
            m3=request.m3,
            calculation=f"{request.initial_grc} + {request.m1} + {m2_value} + {request.m3} = {final_grc}",
            reference="JAR_doc_06 Table 3 - SORA 2.0 AMC"
        )
    except Exception as e:
        logger.error(f"GRC calculation failed: {str(e)}")
        raise HTTPException(status_code=500, detail=str(e))
```

**Action Required**:
- Implement or verify this endpoint exists
- Ensure M2="Medium" is rejected with 400 error
- Ensure M2="Low" works correctly and returns -1 value
- Add logging for debugging

## Testing Requirements

### Unit Tests (Backend_Python/tests/)

**File**: `test_grc_calculator_v20.py`
```python
def test_m2_low_value_correct():
    """Test that M2 Low returns -1 per JAR_doc_06 Table 3"""
    result = calculate_final_grc_sora_20(
        initial_grc=5,
        m1=-1,
        m2=M2Level20.LOW,  # ← This should work
        m3=0
    )
    assert result == 3  # 5 + (-1) + (-1) + 0 = 3

def test_m2_medium_rejected():
    """Test that M2 Medium is rejected (not valid in SORA 2.0)"""
    with pytest.raises(ValueError, match="Invalid M2Level20 value"):
        calculate_final_grc_sora_20(
            initial_grc=5,
            m1=-1,
            m2="Medium",  # ← This must raise error
            m3=0
        )

def test_all_valid_m2_levels():
    """Test all three valid SORA 2.0 M2 levels"""
    test_cases = [
        (M2Level20.NONE, 5, 4),   # 5 + (-1) + 0 + 0 = 4
        (M2Level20.LOW, 5, 3),    # 5 + (-1) + (-1) + 0 = 3
        (M2Level20.HIGH, 5, 2),   # 5 + (-1) + (-2) + 0 = 2
    ]
    
    for m2, initial, expected in test_cases:
        result = calculate_final_grc_sora_20(initial, -1, m2, 0)
        assert result == expected, f"Failed for M2={m2}"
```

**Action Required**:
- Run these tests and ensure all pass
- Add tests for .NET backend as well
- Verify integration tests with live API calls

### Integration Test (Live API)

**Scenario**: User fills out SORA 2.0 form, selects M2="Low", clicks "Execute SORA Evaluation"

**Expected Flow**:
1. Frontend sends: `{ "category": "SORA-2.0", "grc": { "m2": "Low" } }`
2. .NET API validates M2="Low" (✅ valid)
3. .NET API calls Python FastAPI: `POST http://localhost:8001/api/grc/calculate-v20`
4. Python calculates Final GRC with M2=-1
5. Python returns result to .NET
6. .NET continues with ARC/SAIL calculations
7. Frontend displays: Final GRC, Final ARC, SAIL I-VI

**Current Issue**: 400 Bad Request at step 2 or 3

**Action Required**:
- Fix validation logic in .NET
- Ensure Python FastAPI is running
- Test end-to-end with browser DevTools open
- Capture and fix any remaining errors

## Database/Storage Considerations

**Check for hardcoded "Medium" values**:
```sql
-- If using database, check for old M2 values
SELECT * FROM SoraEvaluations WHERE M2Level = 'Medium' AND Category = 'SORA-2.0';

-- Update any legacy records
UPDATE SoraEvaluations 
SET M2Level = 'Low', Notes = 'Corrected from Medium to Low per JAR_doc_06 Table 3'
WHERE M2Level = 'Medium' AND Category = 'SORA-2.0';
```

## Documentation Updates

### Update API Documentation
**File**: `Backend/docs/API.md`
```markdown
### SORA 2.0 GRC Inputs

#### M2 - Impact Reduction Mitigation
**Type**: Enum  
**Valid Values**: 
- `None` (0): No impact reduction system
- `Low` (-1): Basic impact reduction (e.g., parachute, foam)
- `High` (-2): Advanced impact reduction (e.g., certified parachute)

**Note**: ⚠️ "Medium" is **NOT valid** for SORA 2.0. Use SORA 2.5 if Medium level is required.

**Reference**: JAR_doc_06 Table 3 - JARUS SORA 2.0 AMC
```

## Summary of Changes Required

| Component | File | Action | Priority |
|-----------|------|--------|----------|
| Python Enum | `grc_models.py` | ✅ Already correct (None/Low/High) | N/A |
| .NET Enum | `GrcModels.cs` | Verify exists, remove "Medium" if present | **HIGH** |
| Python Calculator | `grc_calculator_v20.py` | Ensure M2.LOW=-1, reject "Medium" | **CRITICAL** |
| .NET Validator | `GrcValidatorV20.cs` | Add validation, reject "Medium" | **CRITICAL** |
| .NET Controller | `SoraController.cs` | Fix 400 error, validate M2 | **CRITICAL** |
| Python FastAPI | `grc_routes.py` | Implement endpoint, validate M2 | **CRITICAL** |
| Python Service | Start on port 8001 | `python -m uvicorn main:app --port 8001` | **CRITICAL** |
| Unit Tests | `test_grc_calculator_v20.py` | Add 3 tests (None/Low/High + reject Medium) | HIGH |
| Integration Test | Browser test | End-to-end SORA 2.0 evaluation | HIGH |
| Documentation | `API.md`, inline comments | Update M2 specs with JAR_doc_06 ref | MEDIUM |

## Expected Outcome

After implementing these changes:

1. ✅ **Frontend**: M2 dropdown shows None/Low/High (already done)
2. ✅ **Backend Validation**: Rejects "Medium" with clear error message
3. ✅ **Python Calculation**: Uses -1 for "Low" mitigation
4. ✅ **User Experience**: SORA 2.0 evaluation completes without 400 errors
5. ✅ **Compliance**: 100% aligned with JAR_doc_06 Table 3

## Official Reference Summary

**JARUS Document**: JAR_doc_06 (SORA 2.0 AMC)  
**Table**: Table 3 - Ground Risk Mitigations  
**M2 Specification**:
- None (0)
- Low (-1) ← **This is the fix**
- High (-2)

**NOT VALID**: Medium (-1) ← **This was the bug**

---

**End of Prompt 1**

**Next Steps**: After confirming this fix is complete, proceed to Prompt 2 (SORA 2.5 UI) and Prompt 3 (SORA 2.5 Backend Calculations).
