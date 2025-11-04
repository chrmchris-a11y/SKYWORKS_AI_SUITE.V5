# Risk Mitigation Algorithms for UAS Operations

## SORA Version Comparison

| Aspect | SORA 2.0 | SORA 2.5 |
|--------|----------|----------|
| Total OSOs | 24 | 17 |
| Risk Calculation | Complex | Simplified |
| SAIL Levels | 0-6 | 0-5 |
| M3 Penalty Impact | More granular | More direct |

## Ground Risk Mitigation

### SORA 2.0 Algorithm
```
GRC = Initial Ground Risk * (1 - Σ(Mitigation Strategies))
Max Reduction: 2 levels
Initial Risk Factors:
- Population density
- Operation area
- UAS characteristics
```

### SORA 2.5 Algorithm
```
GRC = Initial Ground Risk * (1 - Σ(Strategic Mitigations))
Max Reduction: 1-2 levels
Key Considerations:
- Population type
- Operation volume
- Safety systems
```

## Air Risk Mitigation

### SORA 2.0 Calculation
```
ARC Adjustment:
- Strategic mitigations improve ARC
- Each mitigation can improve by 0.5 levels
- Max improvement: ARC-d → ARC-a
```

### SORA 2.5 Calculation
```
ARC Reduction:
- More focused strategic mitigations
- Direct correlation with OSO compliance
- Simplified improvement mechanism
```

## Floor Rule Implementation

### M3 Penalty Application
```
ARC Reduction Sequence:
ARC-d → ARC-c → ARC-b → ARC-a

Penalty Calculation:
- Each M3 penalty reduces ARC by 1 level
- Cannot go below ARC-a
```

## SAIL Recalculation

### SORA 2.0 Method
```
SAIL = MAX(0, ROUND(6 * (Ground Risk + Air Risk) / 2))
- Wider SAIL range (0-6)
- More complex calculation
```

### SORA 2.5 Method
```
SAIL = MAX(0, ROUND(5 * (Ground Risk + Air Risk) / 2))
- Narrower SAIL range (0-5)
- Simplified risk assessment
```

## Evidence Requirements

### Mitigation Documentation
- Risk assessment reports
- Operational volume analysis
- Safety system certifications
- Crew training records

## References
- EASA SORA 2.0 AMC
- JARUS SORA 2.5 Guidelines
- UAS Regulation (EU) 2019/947

**Version:** 1.0
**Last Updated:** 2024-03-15