# TMPR (Tactical Mitigation Performance Requirement) Implementation Guide

## Overview
TMPR provides a systematic approach to risk mitigation in UAS operations across SORA 2.0 and 2.5 standards.

## SORA Versions Comparison

### SORA 2.0 (24 OSOs)
- Reliability Threshold: 0.90
- Required OSOs: #10, #11, #12, #14, #15, #21, #22
- TMPR Systems: 
  - Parachute
  - Flight Termination
  - Basic Geo-Fencing

### SORA 2.5 (17 OSOs)
- Reliability Threshold: 0.95
- Required OSOs: #11, #17, #23
- TMPR Systems:
  - Advanced Parachute
  - Enhanced Geo-Fencing
  - Intelligent Flight Termination

## Validation Flow
```csharp
public async Task<TMPRValidationResult> ValidateTMPR(
    Operation operation, 
    string osoId, 
    string soraVersion)
{
    // Version-specific validation logic
    return soraVersion switch 
    {
        "2.0" => ValidateTMPRForSora20(operation, osoId),
        "2.5" => ValidateTMPRForSora25(operation, osoId),
        _ => throw new ArgumentException("Invalid SORA version")
    };
}
```

## API Endpoint Usage

### Validate TMPR
```json
POST /api/tmpr/validate
{
    "operation": { ... },
    "osoId": "OSO-11",
    "soraVersion": "2.5"
}
```

### Get TMPR Systems
```
GET /api/tmpr/systems/2.5
```

### Get TMPR Requirements
```
GET /api/tmpr/requirements/OSO-11/2.5
```

## Best Practices
- Always specify SORA version
- Maintain ≥ 0.9 reliability factor
- Regular system certification
- Comprehensive documentation

## Compliance Checklist
- ☑️ Identify applicable OSOs
- ☑️ Select appropriate TMPR systems
- ☑️ Validate reliability factor
- ☑️ Document system performance

**Version:** 1.0
**Last Updated:** 2024-03-15
**Compliance:** EASA SORA 2.0 & 2.5