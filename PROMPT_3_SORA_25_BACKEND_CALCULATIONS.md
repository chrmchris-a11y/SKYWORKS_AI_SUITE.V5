# PROMPT 3: SORA 2.5 Backend Integration - ARC/GRC/SAIL Calculation Engine Updates

## Executive Summary
This prompt addresses the **critical integration** of the 5 new SORA 2.5 ARC input fields into the backend calculation engines. Currently:
- ✅ Frontend UI: 5 fields implemented and styled
- ✅ Backend Models: Python enums and ARCInputs25 model exist
- ❌ **Calculation Logic**: Fields are NOT used in GRC/ARC/SAIL calculations
- ❌ **Validation**: 400 Bad Request errors when executing SORA 2.5 evaluation
- ❌ **Python FastAPI**: Not running on port 8001

This prompt provides the complete integration specification with official JARUS formulas and references.

---

## Official JARUS References

### Primary Documents
1. **JARUS SORA 2.5 Main Body** (JAR_doc_25)  
   **Edition**: 2.5  
   **Date**: 22.11.2024  
   **Key Sections**:
   - Step #4: Determination of Initial ARC (page 40)
   - Step #5: Application of Strategic Mitigations to Reduce Initial ARC (page 43)
   - Step #6: Tactical Mitigation Performance Requirement (TMPR)
   - Table 7: SAIL Determination (page 47)

2. **JARUS SORA 2.5 Annex H** (JAR_doc_34)  
   **Title**: SORA Safety Services  
   **Edition**: 2.5  
   **Date**: 22.11.2024  
   **Key Sections**:
   - H.2.3: Air Risk Operations Planning Safety Service (AROPSS)
   - H.2.3.1: Calculating Initial ARC
   - H.2.3.2: Apply Strategic Mitigations to Reduce Initial ARC

3. **JARUS SORA Annex F** (JAR_doc_29)  
   **Title**: Theoretical Basis for Ground Risk Classification and Mitigation  
   **Edition**: 2.5  
   **Date**: 13.05.2024

### Supporting References
- **Annex C** (v1.0): Strategic Mitigations Collision Risk Assessment
- **Annex D** (v1.0): Tactical Mitigations Collision Risk Assessment
- **Annex E** (v2.5): Integrity and Assurance Levels for OSO

---

## The 5 New SORA 2.5 ARC Fields - Calculation Impact

### Field 1: U-space Services Available
**Backend Field**: `u_space_services_available` (bool)  
**Official Reference**: JAR_doc_34 (Annex H), Section H.2.3.2

**Impact on Calculations**:
```python
# When U-space services are available AND operational volume is in U-space airspace
if u_space_services_available and in_u_space_airspace:
    # Apply strategic mitigation credit
    arc_reduction = -1  # One ARC level reduction
    
    # Additional requirements:
    # - Service Level Agreement (SLA) with U-space provider
    # - OSO #13 compliance (external services adequate)
    # - Documented service performance characteristics
```

**Integration Points**:
1. **Initial ARC Calculator**: Check if U-space services reduce collision probability
2. **Residual ARC**: Apply strategic mitigation credit
3. **OSO #13 Validation**: Verify service adequacy documentation

**Formula**:
```
Residual ARC = Initial ARC + Strategic Mitigations
where Strategic Mitigations includes:
  - U-space services credit (if available and approved): -1 ARC
  - Other strategic mitigations (airspace containment, segregation)
```

---

### Field 2: Traffic Density Data Source
**Backend Field**: `traffic_density_data_source` (enum: Empirical/Statistical/Expert)  
**Official Reference**: JAR_doc_25 (Main Body), Step #4

**Impact on Calculations**:
```python
def validate_traffic_density_data_source(
    data_source: TrafficDensityDataSource,
    traffic_density: str  # "Low", "Medium", "High"
) -> ValidationResult:
    """
    Validate that data source is appropriate for traffic density level
    
    Official Rule (JAR_doc_25):
    - Empirical data: Valid for all traffic densities (Low/Medium/High)
    - Statistical data: Valid for all traffic densities (Low/Medium/High)
    - Expert judgment: ONLY valid for LOW traffic density
    
    Rationale: Expert judgment lacks sufficient data for accurate 
    classification of medium/high density airspace.
    """
    if data_source == TrafficDensityDataSource.EXPERT:
        if traffic_density != "Low":
            return ValidationResult.Fail(
                f"Expert judgment data source is only valid for LOW traffic density. "
                f"Current density: {traffic_density}. "
                f"You must use Empirical or Statistical data sources for Medium/High density. "
                f"Reference: JAR_doc_25 Step #4"
            )
    
    return ValidationResult.Success()
```

**Integration Points**:
1. **Initial ARC Determination**: Validate data source before ARC calculation
2. **Robustness Level**: Data source affects assurance level
   - Empirical → High assurance
   - Statistical → Medium assurance
   - Expert → Low assurance (LOW density only)
3. **OSO Requirements**: More robust data requires higher OSO compliance

**Robustness Matrix** (per JAR_doc_25 Table 1):
```
Data Source    | Traffic Density | Assurance Level | OSO Requirements
---------------|-----------------|-----------------|------------------
Empirical      | Low/Med/High   | High            | High robustness OSO
Statistical    | Low/Med/High   | Medium          | Medium robustness OSO
Expert         | Low ONLY       | Low             | Low robustness OSO
```

---

### Field 3: Airspace Containment
**Backend Field**: `airspace_containment` (enum: None/Operational/Certified)  
**Official Reference**: JAR_doc_25 (Main Body), Step #5

**Impact on Calculations**:
```python
def calculate_airspace_containment_credit(
    containment: AirspaceContainment25,
    initial_arc: int
) -> int:
    """
    Calculate ARC reduction credit for airspace containment
    
    Official Reference: JAR_doc_25 Step #5 - Strategic Mitigations
    
    Containment reduces the probability that manned aircraft will 
    enter the operational volume.
    
    Reduction depends on:
    1. Robustness level (None/Operational/Certified)
    2. Initial ARC level (higher ARC = less credit)
    """
    if containment == AirspaceContainment25.NONE:
        return 0  # No reduction
    
    elif containment == AirspaceContainment25.OPERATIONAL:
        # Operational containment (Medium robustness)
        # Examples: NOTAMs, coordinated with ANSP, temporary restrictions
        if initial_arc <= 6:
            return -1  # One ARC level reduction
        else:
            return 0  # Limited effectiveness for very high ARC
    
    elif containment == AirspaceContainment25.CERTIFIED:
        # Certified containment (High robustness)
        # Examples: Permanent restricted zones, certified segregation
        if initial_arc <= 4:
            return -2  # Two ARC levels reduction
        elif initial_arc <= 8:
            return -1  # One ARC level reduction
        else:
            return 0  # Limited effectiveness for very high ARC
    
    return 0
```

**Requirements Matrix**:
```
Containment Level | Requirements | ARC Credit | OSO Impact
------------------|--------------|------------|------------
None              | None         | 0          | Standard OSO
Operational       | - ANSP approval              | -1 (if ARC≤6) | OSO #21 (Medium)
                  | - NOTAMs/TFRs                |               |
                  | - Coordination procedures     |               |
                  | - Monitoring                 |               |
Certified         | - Competent authority approval| -2 (if ARC≤4) | OSO #21 (High)
                  | - Permanent segregation      | -1 (if ARC≤8) |
                  | - Technical barriers         |               |
                  | - Certified procedures       |               |
```

---

### Field 4: Temporal Segregation
**Backend Field**: `temporal_segregation` (bool)  
**Official Reference**: Annex C (Strategic Mitigations) - Chronology-based restrictions

**Impact on Calculations**:
```python
def calculate_temporal_segregation_credit(
    temporal_segregation: bool,
    initial_arc: int,
    traffic_density: str
) -> int:
    """
    Calculate ARC reduction credit for temporal segregation
    
    Official Reference: SORA Annex C - Strategic Mitigation by Operational Restrictions
    
    Temporal segregation means operations during periods when manned 
    aviation activity is significantly reduced (e.g., night ops, non-business hours).
    
    Credit depends on:
    1. Whether segregation is applied
    2. Initial ARC level
    3. Baseline traffic density (must show reduction)
    """
    if not temporal_segregation:
        return 0
    
    # Temporal segregation provides credit if:
    # - Documented evidence of reduced manned traffic during operation times
    # - Procedures to enforce time window compliance
    # - Contingency for time window violations
    
    if initial_arc <= 6:
        # Effective for low to medium ARC
        # Requires substantiation of traffic reduction
        return -1  # One ARC level reduction
    else:
        # Limited effectiveness for very high density
        # Even at night/off-hours, some traffic remains
        return 0
```

**Validation Requirements**:
```python
def validate_temporal_segregation(
    temporal_segregation: bool,
    operational_hours: str,
    traffic_data: dict
) -> ValidationResult:
    """
    Validate temporal segregation claim
    
    Requirements:
    1. ConOps must specify exact time windows
    2. Evidence of reduced traffic during those windows
    3. Procedures to ensure compliance
    4. Contingency for violations
    """
    if not temporal_segregation:
        return ValidationResult.Success()
    
    # Check operational hours specified
    if not operational_hours:
        return ValidationResult.Fail(
            "Temporal segregation requires specification of operational time windows in ConOps"
        )
    
    # Check traffic reduction evidence
    if not traffic_data.get('reduced_during_ops'):
        return ValidationResult.Fail(
            "Temporal segregation requires evidence that manned traffic is "
            "significantly reduced during operational time windows"
        )
    
    return ValidationResult.Success()
```

---

### Field 5: Spatial Segregation
**Backend Field**: `spatial_segregation` (bool)  
**Official Reference**: Annex C (Strategic Mitigations) - Boundary-based restrictions

**Impact on Calculations**:
```python
def calculate_spatial_segregation_credit(
    spatial_segregation: bool,
    initial_arc: int,
    operational_volume: dict
) -> int:
    """
    Calculate ARC reduction credit for spatial segregation
    
    Official Reference: SORA Annex C - Strategic Mitigation by Operational Restrictions
    
    Spatial segregation means operations in airspace with defined boundaries 
    that separate from manned aviation (e.g., below minimum safe altitude, 
    lateral separation from airways, vertical separation).
    
    Credit depends on:
    1. Whether segregation is applied
    2. Initial ARC level
    3. Quality of spatial separation (altitude, lateral, vertical)
    """
    if not spatial_segregation:
        return 0
    
    # Spatial segregation provides credit if:
    # - Clearly defined operational boundaries
    # - Analysis showing separation from manned traffic routes
    # - Compliance with airspace structure
    # - Monitoring for containment within boundaries
    
    if initial_arc <= 6:
        # Effective for low to medium ARC
        # Requires substantiation of spatial separation
        return -1  # One ARC level reduction
    else:
        # Limited effectiveness for very high density
        # Difficult to achieve full separation in congested airspace
        return 0
```

**Validation Requirements**:
```python
def validate_spatial_segregation(
    spatial_segregation: bool,
    operational_volume: dict,
    airspace_analysis: dict
) -> ValidationResult:
    """
    Validate spatial segregation claim
    
    Requirements:
    1. ConOps must define exact operational boundaries
    2. Analysis showing separation from manned traffic
    3. Compliance with airspace structure (CTR, ATZ, etc.)
    4. Containment monitoring procedures
    """
    if not spatial_segregation:
        return ValidationResult.Success()
    
    # Check operational boundaries defined
    if not operational_volume.get('boundaries_defined'):
        return ValidationResult.Fail(
            "Spatial segregation requires clearly defined operational boundaries in ConOps"
        )
    
    # Check separation analysis
    if not airspace_analysis.get('manned_traffic_separation'):
        return ValidationResult.Fail(
            "Spatial segregation requires analysis showing separation from manned traffic routes"
        )
    
    return ValidationResult.Success()
```

---

## Complete SORA 2.5 ARC Calculation Flow

### Step 1: Determine Initial ARC (Step #4)

**File**: `Backend_Python/arc/calculators/initial_arc_calculator_v25.py`

```python
from models.arc_models import ARCInputs25, TrafficDensityDataSource, AirspaceContainment25

def calculate_initial_arc_v25(
    operational_volume: dict,
    traffic_density: str,  # "Low", "Medium", "High"
    arc_inputs: ARCInputs25
) -> tuple[int, str]:
    """
    Calculate Initial ARC for SORA 2.5
    
    Official Reference: JAR_doc_25 Step #4
    
    Initial ARC depends on:
    1. Intrinsic airspace characteristics (traffic density, airspace class)
    2. Operational volume (size, altitude, proximity to airports)
    3. Data source reliability (affects robustness)
    
    Returns:
        (initial_arc_value, explanation)
    """
    # Validate traffic density data source
    validation = validate_traffic_density_data_source(
        arc_inputs.traffic_density_data_source,
        traffic_density
    )
    if not validation.is_valid:
        raise ValueError(validation.error_message)
    
    # Base Initial ARC from traffic density and operational characteristics
    # This uses the standard SORA table (similar to SORA 2.0)
    base_arc = calculate_base_arc(operational_volume, traffic_density)
    
    # Data source affects assurance level (not directly ARC value)
    # But it impacts which OSO robustness levels are required
    assurance_level = get_assurance_level(arc_inputs.traffic_density_data_source)
    
    explanation = (
        f"Initial ARC: {base_arc} (based on {traffic_density} traffic density)\\n"
        f"Data Source: {arc_inputs.traffic_density_data_source.value} "
        f"(Assurance Level: {assurance_level})"
    )
    
    return base_arc, explanation

def calculate_base_arc(operational_volume: dict, traffic_density: str) -> int:
    """
    Calculate base ARC using standard SORA table
    
    Simplified table (full implementation should use JAR_doc_25 Table):
    """
    arc_table = {
        ("Low", "Low_Altitude"): 1,      # Below 400 ft AGL, low traffic
        ("Low", "Medium_Altitude"): 2,
        ("Medium", "Low_Altitude"): 3,
        ("Medium", "Medium_Altitude"): 4,
        ("Medium", "High_Altitude"): 5,
        ("High", "Low_Altitude"): 5,
        ("High", "Medium_Altitude"): 6,
        ("High", "High_Altitude"): 8,
    }
    
    altitude_band = categorize_altitude(operational_volume['max_altitude'])
    key = (traffic_density, altitude_band)
    
    return arc_table.get(key, 4)  # Default to ARC 4 if not in table

def get_assurance_level(data_source: TrafficDensityDataSource) -> str:
    """Map data source to assurance level"""
    mapping = {
        TrafficDensityDataSource.EMPIRICAL: "High",
        TrafficDensityDataSource.STATISTICAL: "Medium",
        TrafficDensityDataSource.EXPERT: "Low"
    }
    return mapping[data_source]
```

---

### Step 2: Apply Strategic Mitigations (Step #5)

**File**: `Backend_Python/arc/calculators/strategic_mitigations_v25.py`

```python
def calculate_residual_arc_v25(
    initial_arc: int,
    arc_inputs: ARCInputs25,
    operational_volume: dict
) -> tuple[int, list[str]]:
    """
    Calculate Residual ARC after applying strategic mitigations
    
    Official Reference: JAR_doc_25 Step #5
    
    Formula:
        Residual ARC = Initial ARC + Strategic Mitigation Credits
    
    Strategic Mitigations:
    1. U-space Services
    2. Airspace Containment
    3. Temporal Segregation
    4. Spatial Segregation
    
    Returns:
        (residual_arc, list_of_applied_mitigations)
    """
    mitigations = []
    total_credit = 0
    
    # 1. U-space Services
    if arc_inputs.u_space_services_available:
        credit = calculate_u_space_credit(initial_arc, operational_volume)
        total_credit += credit
        if credit < 0:
            mitigations.append(
                f"U-space Services: {credit} ARC (JAR_doc_34 Annex H)"
            )
    
    # 2. Airspace Containment
    containment_credit = calculate_airspace_containment_credit(
        arc_inputs.airspace_containment,
        initial_arc
    )
    total_credit += containment_credit
    if containment_credit < 0:
        mitigations.append(
            f"Airspace Containment ({arc_inputs.airspace_containment.value}): "
            f"{containment_credit} ARC (JAR_doc_25 Step #5)"
        )
    
    # 3. Temporal Segregation
    if arc_inputs.temporal_segregation:
        temporal_credit = calculate_temporal_segregation_credit(
            arc_inputs.temporal_segregation,
            initial_arc,
            operational_volume.get('traffic_density', 'Medium')
        )
        total_credit += temporal_credit
        if temporal_credit < 0:
            mitigations.append(
                f"Temporal Segregation: {temporal_credit} ARC (Annex C)"
            )
    
    # 4. Spatial Segregation
    if arc_inputs.spatial_segregation:
        spatial_credit = calculate_spatial_segregation_credit(
            arc_inputs.spatial_segregation,
            initial_arc,
            operational_volume
        )
        total_credit += spatial_credit
        if spatial_credit < 0:
            mitigations.append(
                f"Spatial Segregation: {spatial_credit} ARC (Annex C)"
            )
    
    # Calculate Residual ARC (cannot be less than 1)
    residual_arc = max(1, initial_arc + total_credit)
    
    # Add summary
    mitigations.append(
        f"\\nResidual ARC = Initial ARC ({initial_arc}) + "
        f"Strategic Credits ({total_credit}) = {residual_arc}"
    )
    
    return residual_arc, mitigations

def calculate_u_space_credit(initial_arc: int, operational_volume: dict) -> int:
    """
    Calculate U-space services strategic mitigation credit
    
    Credit depends on:
    1. Service availability (already checked)
    2. Operational volume in U-space airspace
    3. Initial ARC level
    """
    # Check if operational volume is in U-space designated airspace
    if not operational_volume.get('in_u_space_airspace', False):
        return 0  # No credit if not in U-space airspace
    
    # U-space services provide strategic deconfliction
    if initial_arc <= 6:
        return -1  # One ARC level reduction
    else:
        return 0  # Limited effectiveness for very high ARC
```

---

### Step 3: Determine Final SAIL (Step #9)

**File**: `Backend_Python/sail/sail_calculator_v25.py`

```python
def calculate_sail_v25(
    final_grc: int,
    residual_arc: int
) -> tuple[str, str]:
    """
    Determine SAIL for SORA 2.5
    
    Official Reference: JAR_doc_25 Table 7 (page 47)
    
    SAIL depends on:
    1. Final GRC (after all ground risk mitigations)
    2. Residual ARC (after strategic mitigations)
    
    Returns:
        (sail_level, explanation)
    """
    # SORA 2.5 SAIL Table (JAR_doc_25 Table 7)
    # Rows = Final GRC (1-10), Columns = Residual ARC (1-10)
    sail_table = {
        (1, 1): "I", (1, 2): "I", (1, 3): "II", (1, 4): "II",
        (2, 1): "I", (2, 2): "II", (2, 3): "II", (2, 4): "III",
        (3, 1): "II", (3, 2): "II", (3, 3): "III", (3, 4): "IV",
        (4, 1): "II", (4, 2): "III", (4, 3): "IV", (4, 4): "V",
        (5, 1): "III", (5, 2): "IV", (5, 3): "V", (5, 4): "VI",
        (6, 1): "IV", (6, 2): "V", (6, 3): "VI", (6, 4): "VI",
        # ... (full table in JAR_doc_25)
    }
    
    # Clamp values to table bounds
    grc_clamped = min(10, max(1, final_grc))
    arc_clamped = min(10, max(1, residual_arc))
    
    key = (grc_clamped, arc_clamped)
    sail = sail_table.get(key, "VI")  # Default to highest SAIL if not in table
    
    explanation = (
        f"SAIL {sail}: Determined from Final GRC={grc_clamped} and "
        f"Residual ARC={arc_clamped}\\n"
        f"Reference: JAR_doc_25 Table 7"
    )
    
    return sail, explanation
```

---

## Backend Files Requiring Updates

### Python Backend Structure

```
Backend_Python/
├── models/
│   ├── arc_models.py                    ✅ Already correct (ARCInputs25)
│   └── grc_models.py                    ✅ Already correct (M2Level25)
├── arc/
│   ├── calculators/
│   │   ├── initial_arc_calculator_v25.py    ❌ CREATE NEW
│   │   ├── strategic_mitigations_v25.py     ❌ CREATE NEW
│   │   └── residual_arc_calculator_v25.py   ❌ CREATE NEW
│   └── validators/
│       └── arc_validator_v25.py             ❌ CREATE NEW
├── grc/
│   └── calculators/
│       └── grc_calculator_v25.py            ✅ May already exist
├── sail/
│   └── sail_calculator_v25.py               ❌ UPDATE to use new ARC
├── routes/
│   ├── sora_routes.py                       ❌ UPDATE to call new calculators
│   └── arc_routes.py                        ❌ CREATE NEW (for testing)
└── main.py                                  ❌ ENSURE RUNNING on port 8001
```

---

### File 1: `Backend_Python/arc/calculators/initial_arc_calculator_v25.py`

**Status**: ❌ **CREATE NEW**

**Purpose**: Calculate Initial ARC using SORA 2.5 methodology with traffic density data source validation

**Key Functions**:
```python
def calculate_initial_arc_v25(
    operational_volume: dict,
    traffic_density: str,
    arc_inputs: ARCInputs25
) -> tuple[int, str]:
    """Full implementation as shown in Step 1 above"""
    pass

def validate_traffic_density_data_source(
    data_source: TrafficDensityDataSource,
    traffic_density: str
) -> ValidationResult:
    """Validate Expert source only for Low density"""
    pass

def calculate_base_arc(
    operational_volume: dict,
    traffic_density: str
) -> int:
    """Use JAR_doc_25 ARC table"""
    pass

def get_assurance_level(
    data_source: TrafficDensityDataSource
) -> str:
    """Map source to High/Medium/Low assurance"""
    pass
```

---

### File 2: `Backend_Python/arc/calculators/strategic_mitigations_v25.py`

**Status**: ❌ **CREATE NEW**

**Purpose**: Apply all 5 strategic mitigations to reduce Initial ARC

**Key Functions**:
```python
def calculate_residual_arc_v25(
    initial_arc: int,
    arc_inputs: ARCInputs25,
    operational_volume: dict
) -> tuple[int, list[str]]:
    """Full implementation as shown in Step 2 above"""
    pass

def calculate_u_space_credit(
    initial_arc: int,
    operational_volume: dict
) -> int:
    """U-space services credit"""
    pass

def calculate_airspace_containment_credit(
    containment: AirspaceContainment25,
    initial_arc: int
) -> int:
    """Airspace containment credit"""
    pass

def calculate_temporal_segregation_credit(
    temporal_segregation: bool,
    initial_arc: int,
    traffic_density: str
) -> int:
    """Temporal segregation credit"""
    pass

def calculate_spatial_segregation_credit(
    spatial_segregation: bool,
    initial_arc: int,
    operational_volume: dict
) -> int:
    """Spatial segregation credit"""
    pass
```

---

### File 3: `Backend_Python/sail/sail_calculator_v25.py`

**Status**: ❌ **UPDATE** (may already exist but needs new ARC integration)

**Purpose**: Determine SAIL I-VI using JAR_doc_25 Table 7

**Key Functions**:
```python
def calculate_sail_v25(
    final_grc: int,
    residual_arc: int
) -> tuple[str, str]:
    """Full implementation as shown in Step 3 above"""
    pass

def get_oso_requirements(sail: str) -> list[dict]:
    """Return OSO requirements for given SAIL level"""
    # Reference: JAR_doc_25 Annex E
    pass
```

---

### File 4: `Backend_Python/routes/sora_routes.py`

**Status**: ❌ **UPDATE** (add SORA 2.5 endpoint)

**Required Endpoint**:
```python
@router.post("/api/sora/complete-v25")
async def calculate_complete_sora_v25(request: SoraRequestV25):
    """
    Complete SORA 2.5 evaluation with enhanced ARC inputs
    
    Official Reference: JAR_doc_25 (SORA 2.5 Main Body)
    """
    try:
        # Step #2: Calculate Initial GRC (similar to SORA 2.0)
        initial_grc = calculate_initial_grc_v25(
            request.grc_inputs,
            request.operational_volume
        )
        
        # Step #3: Apply GRC mitigations
        final_grc = calculate_final_grc_v25(
            initial_grc,
            request.grc_inputs
        )
        
        # Step #4: Calculate Initial ARC (NEW - uses 5 fields)
        initial_arc, arc_explanation = calculate_initial_arc_v25(
            request.operational_volume,
            request.traffic_density,
            request.arc_inputs_25  # ← This is the key difference
        )
        
        # Step #5: Apply Strategic Mitigations (NEW - uses 5 fields)
        residual_arc, mitigations = calculate_residual_arc_v25(
            initial_arc,
            request.arc_inputs_25,  # ← This is where 5 fields are used
            request.operational_volume
        )
        
        # Step #9: Determine SAIL
        sail, sail_explanation = calculate_sail_v25(
            final_grc,
            residual_arc
        )
        
        # Return complete results
        return SoraResponseV25(
            initial_grc=initial_grc,
            final_grc=final_grc,
            initial_arc=initial_arc,
            residual_arc=residual_arc,
            sail=sail,
            strategic_mitigations=mitigations,
            arc_explanation=arc_explanation,
            sail_explanation=sail_explanation,
            reference="JAR_doc_25 - SORA 2.5 Main Body"
        )
    
    except ValueError as e:
        logger.error(f"Validation error: {str(e)}")
        raise HTTPException(status_code=400, detail=str(e))
    except Exception as e:
        logger.error(f"Calculation error: {str(e)}")
        raise HTTPException(status_code=500, detail=str(e))
```

---

## .NET Backend Integration

### File 1: `Backend/src/Skyworks.Core/Services/ARC/ArcCalculatorV25.cs`

**Status**: ❌ **CREATE NEW**

**Purpose**: .NET wrapper to call Python FastAPI for ARC calculations

```csharp
public class ArcCalculatorV25 : IArcCalculator
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ArcCalculatorV25> _logger;
    
    public ArcCalculatorV25(HttpClient httpClient, ILogger<ArcCalculatorV25> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }
    
    public async Task<ArcResult> CalculateArcAsync(ArcInputsV25 inputs)
    {
        try
        {
            // Call Python FastAPI endpoint
            var response = await _httpClient.PostAsJsonAsync(
                "http://localhost:8001/api/arc/calculate-v25",
                inputs
            );
            
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError($"Python ARC calculation failed: {error}");
                throw new InvalidOperationException(
                    $"ARC calculation failed: {error}. " +
                    "Ensure Python FastAPI is running on port 8001."
                );
            }
            
            var result = await response.Content.ReadFromJsonAsync<ArcResult>();
            return result;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError($"Cannot connect to Python service: {ex.Message}");
            throw new InvalidOperationException(
                "Python FastAPI service is not running on port 8001. " +
                "Please start the service: python -m uvicorn main:app --port 8001",
                ex
            );
        }
    }
}
```

---

### File 2: `Backend/src/Skyworks.Api/Controllers/SoraController.cs`

**Status**: ❌ **UPDATE** (add SORA 2.5 handling)

```csharp
[HttpPost("complete")]
public async Task<IActionResult> CalculateCompleteSora([FromBody] SoraRequest request)
{
    try
    {
        if (request.Category == "SORA-2.5")
        {
            // Validate that arc_inputs_25 is provided
            if (request.ArcInputs25 == null)
            {
                return BadRequest(new {
                    error = "Missing SORA 2.5 ARC inputs",
                    details = "SORA 2.5 requires the following enhanced ARC fields: " +
                              "u_space_services_available, traffic_density_data_source, " +
                              "airspace_containment, temporal_segregation, spatial_segregation",
                    reference = "JAR_doc_25 Steps #4 and #5"
                });
            }
            
            // Validate traffic density data source
            if (request.ArcInputs25.TrafficDensityDataSource == TrafficDensityDataSource.Expert 
                && request.TrafficDensity != "Low")
            {
                return BadRequest(new {
                    error = "Invalid traffic density data source",
                    details = "Expert judgment is only valid for LOW traffic density. " +
                              $"Current density: {request.TrafficDensity}. " +
                              "Use Empirical or Statistical data sources for Medium/High density.",
                    reference = "JAR_doc_25 Step #4"
                });
            }
            
            // Calculate SORA 2.5 (calls Python FastAPI)
            var result = await _soraCalculatorV25.CalculateCompleteAsync(request);
            
            return Ok(result);
        }
        else if (request.Category == "SORA-2.0")
        {
            // SORA 2.0 logic (existing code)
            var result = await _soraCalculatorV20.CalculateCompleteAsync(request);
            return Ok(result);
        }
        else
        {
            return BadRequest(new {
                error = "Invalid SORA category",
                details = $"Category '{request.Category}' is not supported. Use 'SORA-2.0' or 'SORA-2.5'."
            });
        }
    }
    catch (InvalidOperationException ex)
    {
        _logger.LogError($"Calculation error: {ex.Message}");
        return StatusCode(500, new {
            error = "Calculation failed",
            details = ex.Message
        });
    }
}
```

---

## Testing Requirements

### Unit Tests

**File**: `Backend_Python/tests/test_arc_calculator_v25.py`

```python
import pytest
from models.arc_models import ARCInputs25, TrafficDensityDataSource, AirspaceContainment25
from arc.calculators.initial_arc_calculator_v25 import calculate_initial_arc_v25
from arc.calculators.strategic_mitigations_v25 import calculate_residual_arc_v25

def test_expert_data_source_rejected_for_medium_density():
    """Test that Expert source is rejected for Medium traffic density"""
    arc_inputs = ARCInputs25(
        u_space_services_available=False,
        traffic_density_data_source=TrafficDensityDataSource.EXPERT,
        airspace_containment=AirspaceContainment25.NONE,
        temporal_segregation=False,
        spatial_segregation=False
    )
    
    with pytest.raises(ValueError, match="only valid for LOW traffic density"):
        calculate_initial_arc_v25(
            operational_volume={'max_altitude': 120},
            traffic_density="Medium",  # ← This should fail
            arc_inputs=arc_inputs
        )

def test_u_space_services_credit():
    """Test that U-space services provide ARC credit"""
    arc_inputs = ARCInputs25(
        u_space_services_available=True,  # ← U-space enabled
        traffic_density_data_source=TrafficDensityDataSource.EMPIRICAL,
        airspace_containment=AirspaceContainment25.NONE,
        temporal_segregation=False,
        spatial_segregation=False
    )
    
    initial_arc = 4
    operational_volume = {'in_u_space_airspace': True}
    
    residual_arc, mitigations = calculate_residual_arc_v25(
        initial_arc,
        arc_inputs,
        operational_volume
    )
    
    # Should get -1 ARC credit
    assert residual_arc == 3  # 4 + (-1) = 3
    assert any("U-space" in m for m in mitigations)

def test_airspace_containment_certified_credit():
    """Test that Certified containment provides -2 ARC credit for low ARC"""
    arc_inputs = ARCInputs25(
        u_space_services_available=False,
        traffic_density_data_source=TrafficDensityDataSource.EMPIRICAL,
        airspace_containment=AirspaceContainment25.CERTIFIED,  # ← Certified
        temporal_segregation=False,
        spatial_segregation=False
    )
    
    initial_arc = 4  # Low enough for -2 credit
    operational_volume = {}
    
    residual_arc, mitigations = calculate_residual_arc_v25(
        initial_arc,
        arc_inputs,
        operational_volume
    )
    
    # Should get -2 ARC credit
    assert residual_arc == 2  # 4 + (-2) = 2
    assert any("Certified" in m for m in mitigations)

def test_temporal_and_spatial_segregation_stack():
    """Test that temporal and spatial segregation can both apply"""
    arc_inputs = ARCInputs25(
        u_space_services_available=False,
        traffic_density_data_source=TrafficDensityDataSource.STATISTICAL,
        airspace_containment=AirspaceContainment25.NONE,
        temporal_segregation=True,   # ← Both enabled
        spatial_segregation=True     # ← Both enabled
    )
    
    initial_arc = 5
    operational_volume = {'traffic_density': 'Low'}
    
    residual_arc, mitigations = calculate_residual_arc_v25(
        initial_arc,
        arc_inputs,
        operational_volume
    )
    
    # Should get -1 for temporal + -1 for spatial = -2 total
    assert residual_arc == 3  # 5 + (-2) = 3
    assert any("Temporal" in m for m in mitigations)
    assert any("Spatial" in m for m in mitigations)

def test_no_mitigations_residual_equals_initial():
    """Test that with no mitigations, Residual ARC = Initial ARC"""
    arc_inputs = ARCInputs25(
        u_space_services_available=False,
        traffic_density_data_source=TrafficDensityDataSource.EMPIRICAL,
        airspace_containment=AirspaceContainment25.NONE,
        temporal_segregation=False,
        spatial_segregation=False
    )
    
    initial_arc = 6
    operational_volume = {}
    
    residual_arc, mitigations = calculate_residual_arc_v25(
        initial_arc,
        arc_inputs,
        operational_volume
    )
    
    assert residual_arc == initial_arc
    assert "0" in mitigations[-1]  # Total credit should be 0
```

---

### Integration Tests

**Test Scenario**: End-to-end SORA 2.5 evaluation with all 5 fields

**Steps**:
1. Start Python FastAPI: `python -m uvicorn main:app --port 8001`
2. Verify health: `curl http://localhost:8001/health` → 200 OK
3. Start .NET API: `dotnet run --project Skyworks.Api.csproj --urls http://localhost:5210`
4. Open mission.html in browser
5. Select "SORA-2.5" category
6. Fill out all fields:
   - U-space Services: Yes
   - Traffic Density Source: Empirical
   - Airspace Containment: Operational
   - Temporal Segregation: ☑ Checked
   - Spatial Segregation: ☑ Checked
7. Click "Execute SORA Evaluation"
8. **Expected Result**:
   - ✅ No 400 errors
   - ✅ Initial ARC calculated
   - ✅ Residual ARC shows strategic mitigation credits
   - ✅ SAIL I-VI determined
   - ✅ Results displayed in UI

---

## Summary of Critical Changes

| Priority | Component | Action Required |
|----------|-----------|-----------------|
| **CRITICAL** | Python FastAPI | Start service on port 8001 |
| **CRITICAL** | `initial_arc_calculator_v25.py` | Create new file with data source validation |
| **CRITICAL** | `strategic_mitigations_v25.py` | Create new file with 5-field integration |
| **CRITICAL** | `sora_routes.py` | Add `/api/sora/complete-v25` endpoint |
| **CRITICAL** | `SoraController.cs` | Add SORA 2.5 handling with 5-field validation |
| HIGH | `sail_calculator_v25.py` | Update to use new Residual ARC |
| HIGH | `ArcCalculatorV25.cs` | Create .NET wrapper for Python calls |
| HIGH | Unit Tests | Add 5+ tests for strategic mitigations |
| MEDIUM | Integration Tests | End-to-end browser test |

---

## Expected Outcome

After implementing these changes:

1. ✅ **Python FastAPI Running**: Port 8001 health check passes
2. ✅ **Data Source Validation**: Expert rejected for Medium/High density
3. ✅ **U-space Credit**: -1 ARC when U-space services available
4. ✅ **Containment Credit**: -1 or -2 ARC based on robustness
5. ✅ **Segregation Credits**: -1 ARC each for temporal/spatial
6. ✅ **SAIL Determination**: Correct SAIL I-VI based on Final GRC + Residual ARC
7. ✅ **No 400 Errors**: SORA 2.5 evaluation completes successfully
8. ✅ **100% JARUS Compliance**: All calculations per JAR_doc_25

---

**End of Prompt 3**

**All 3 prompts are now complete. Send them to Sonnet ONE AT A TIME in this order:**
1. PROMPT_1_SORA_20_M2_BACKEND_INTEGRATION.md (simplest)
2. PROMPT_2_SORA_25_UI_IMPLEMENTATION.md (UI polish)
3. PROMPT_3_SORA_25_BACKEND_CALCULATIONS.md (most complex)

Wait for Sonnet to complete each prompt before sending the next one.
