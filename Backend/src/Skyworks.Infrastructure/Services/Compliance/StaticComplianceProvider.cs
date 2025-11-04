using Skyworks.Core.Models.Compliance;
using Skyworks.Core.Services.Compliance;

namespace Skyworks.Infrastructure.Services.Compliance;

/// <summary>
/// Static compliance provider με template obligations και checklists
/// Βασισμένο σε SORA 2.5, EASA regulations, και Cyprus/Greece specifics
/// </summary>
public class StaticComplianceProvider : IComplianceProvider
{
    public Task<ComplianceResult> GetComplianceObligationsAsync(ComplianceRequest request)
    {
        var result = new ComplianceResult();

        // Airspace-based obligations
        AddAirspaceObligations(request, result);

        // ARC-based obligations (OSO, TMPR)
        AddARCBasedObligations(request, result);

        // U-space obligations
        if (request.IsUSpaceAirspace)
        {
            AddUSpaceObligations(request, result);
        }

        // BVLOS obligations
        if (request.OperationType?.Contains("BVLOS", StringComparison.OrdinalIgnoreCase) == true)
        {
            AddBVLOSObligations(request, result);
        }

        // Special use areas
        AddSpecialUseAreaObligations(request, result);

        // Determine overall status
        result.Status = DetermineComplianceStatus(result);

        return Task.FromResult(result);
    }

    public Task<OperatorReadiness> ValidateOperatorReadinessAsync(string operatorId, string missionId, ComplianceRequest request)
    {
        var readiness = new OperatorReadiness
        {
            OperatorId = operatorId,
            MissionId = missionId
        };

        // Required OSOs based on ARC
        readiness.RequiredOSOs = GetRequiredOSOs(request.ARC);

        // Required TMPR based on ARC
        readiness.RequiredTMPR = GetRequiredTMPR(request.ARC);

        // Documentation checklist
        readiness.DocumentationChecklist = GetDocumentationChecklist(request);

        // Equipment checklist
        readiness.EquipmentChecklist = GetEquipmentChecklist(request);

        // Validation notes
        ValidateReadiness(readiness);

        return Task.FromResult(readiness);
    }

    public Task<List<ComplianceChecklistItem>> GetPreFlightChecklistAsync(ComplianceRequest request)
    {
        var checklist = new List<ComplianceChecklistItem>();

        // Weather check
        checklist.Add(new ComplianceChecklistItem
        {
            Category = "Weather",
            Item = "Verify VMC conditions (visibility ≥5km, cloud ceiling ≥450m below 3000ft)",
            IsMandatory = true,
            ReferenceSource = "EASA Easy Access Rules SERA"
        });

        // NOTAM check
        checklist.Add(new ComplianceChecklistItem
        {
            Category = "Airspace",
            Item = "Review active NOTAMs for area of operation",
            IsMandatory = true,
            ReferenceSource = "ICAO Annex 15"
        });

        // Airspace authorization
        if (request.AirspaceType == "CTR" || request.AirspaceType == "ATZ" || request.AirspaceType == "TMA")
        {
            checklist.Add(new ComplianceChecklistItem
            {
                Category = "Airspace",
                Item = $"Obtain ATC clearance for {request.AirspaceType} operation",
                IsMandatory = true,
                Notes = "Submit request at least 48h in advance",
                ReferenceSource = "EASA Regulation (EU) 2019/947"
            });
        }

        // U-space
        if (request.IsUSpaceAirspace)
        {
            checklist.Add(new ComplianceChecklistItem
            {
                Category = "Airspace",
                Item = "Register flight plan in U-space service provider",
                IsMandatory = true,
                ReferenceSource = "EASA Regulation (EU) 2021/664"
            });
        }

        // Equipment checks
        checklist.Add(new ComplianceChecklistItem
        {
            Category = "Equipment",
            Item = "Verify Remote ID transmission (if required)",
            IsMandatory = request.ARC == "c" || request.ARC == "d",
            Notes = "Required for ARC-c and ARC-d",
            ReferenceSource = "EASA AMC/GM to Regulation (EU) 2019/945"
        });

        checklist.Add(new ComplianceChecklistItem
        {
            Category = "Equipment",
            Item = "Test geo-fencing system",
            IsMandatory = true,
            ReferenceSource = "SORA 2.5 OSO#3"
        });

        // BVLOS-specific
        if (request.OperationType?.Contains("BVLOS", StringComparison.OrdinalIgnoreCase) == true)
        {
            checklist.Add(new ComplianceChecklistItem
            {
                Category = "Equipment",
                Item = "Verify detect-and-avoid (DAA) system operational",
                IsMandatory = true,
                Notes = "Required for BVLOS operations",
                ReferenceSource = "SORA 2.5 OSO#18"
            });

            checklist.Add(new ComplianceChecklistItem
            {
                Category = "Personnel",
                Item = "Confirm remote pilot BVLOS certification",
                IsMandatory = true,
                ReferenceSource = "EASA Regulation (EU) 2019/947 Part-UAS"
            });
        }

        // Documentation
        checklist.Add(new ComplianceChecklistItem
        {
            Category = "Documentation",
            Item = "Operations Manual available and current",
            IsMandatory = request.ARC == "c" || request.ARC == "d",
            ReferenceSource = "SORA 2.5 OSO#2"
        });

        checklist.Add(new ComplianceChecklistItem
        {
            Category = "Documentation",
            Item = "Risk assessment (SORA) approved by authority",
            IsMandatory = request.ARC == "c" || request.ARC == "d",
            Notes = "For specific category operations",
            ReferenceSource = "EASA Regulation (EU) 2019/947 Article 11"
        });

        return Task.FromResult(checklist);
    }

    #region Private Helpers

    private void AddAirspaceObligations(ComplianceRequest request, ComplianceResult result)
    {
        if (request.AirspaceType == "CTR" || request.AirspaceType == "ATZ")
        {
            result.Obligations.Add(new ComplianceObligation
            {
                Type = "ATC",
                Description = $"ATC clearance required for {request.AirspaceType} operation",
                DetailedGuidance = "Submit request to local ATC at least 48 hours before operation. Include flight plan, drone specs, and pilot credentials.",
                Priority = "Mandatory",
                ReferenceSource = "EASA Regulation (EU) 2019/947, Article 15",
                Deadline = "48h before operation"
            });
        }

        if (request.AirspaceType == "TMA")
        {
            result.Obligations.Add(new ComplianceObligation
            {
                Type = "Airspace Auth",
                Description = "TMA coordination with ATC required",
                DetailedGuidance = "Coordinate with Terminal Control for lateral/vertical separation. May require transponder or ADS-B out.",
                Priority = "Mandatory",
                ReferenceSource = "ICAO Annex 2"
            });
        }

        if (request.AirspaceType == "Restricted" || request.AirspaceType == "Danger" || request.AirspaceType == "Prohibited")
        {
            result.Obligations.Add(new ComplianceObligation
            {
                Type = "Airspace Auth",
                Description = $"Special authorization required for {request.AirspaceType} airspace",
                DetailedGuidance = "Contact managing authority for special use airspace. Operation may be prohibited or require military/civil coordination.",
                Priority = "Mandatory",
                ReferenceSource = "ICAO Annex 11"
            });
        }
    }

    private void AddARCBasedObligations(ComplianceRequest request, ComplianceResult result)
    {
        if (request.ARC == "c" || request.ARC == "d")
        {
            result.Obligations.Add(new ComplianceObligation
            {
                Type = "OSO",
                Description = $"Operational Safety Objectives (OSOs) required for ARC-{request.ARC}",
                DetailedGuidance = "Implement all required OSOs per SORA 2.5 Annex E. See OSO matrix for ARC-specific requirements.",
                Priority = "Mandatory",
                ReferenceSource = "SORA 2.5 Annex E"
            });

            result.Obligations.Add(new ComplianceObligation
            {
                Type = "TMPR",
                Description = $"Tactical Mitigation Performance Required (TMPR) for ARC-{request.ARC}",
                DetailedGuidance = "TMPR level must match ARC: ARC-c → TMPR 2-3, ARC-d → TMPR 3-4. Implement detect-and-avoid.",
                Priority = "Mandatory",
                ReferenceSource = "SORA 2.5 Section 5"
            });

            result.Obligations.Add(new ComplianceObligation
            {
                Type = "Documentation",
                Description = "Operations Manual and Risk Assessment required",
                DetailedGuidance = "Prepare detailed Operations Manual (OpManual) and submit SORA risk assessment to CAA for approval.",
                Priority = "Mandatory",
                ReferenceSource = "EASA Regulation (EU) 2019/947, Article 11",
                Deadline = "Before first flight"
            });
        }
    }

    private void AddUSpaceObligations(ComplianceRequest request, ComplianceResult result)
    {
        result.Obligations.Add(new ComplianceObligation
        {
            Type = "U-space",
            Description = "U-space registration and flight plan submission",
            DetailedGuidance = "Register with designated U-space Service Provider (USSP). Submit flight plan including trajectory, altitude, time window.",
            Priority = "Mandatory",
            ReferenceSource = "EASA Regulation (EU) 2021/664",
            RelatedLinks = new List<string>
            {
                "https://www.easa.europa.eu/en/domains/civil-drones/u-space"
            }
        });

        result.Obligations.Add(new ComplianceObligation
        {
            Type = "U-space",
            Description = "Real-time tracking and telemetry",
            DetailedGuidance = "Maintain continuous position broadcast to USSP. Remote ID transmission required.",
            Priority = "Mandatory",
            ReferenceSource = "EASA Regulation (EU) 2021/664, Annex"
        });
    }

    private void AddBVLOSObligations(ComplianceRequest request, ComplianceResult result)
    {
        result.Obligations.Add(new ComplianceObligation
        {
            Type = "BVLOS",
            Description = "BVLOS authorization from CAA",
            DetailedGuidance = "Submit BVLOS authorization application including ConOps, risk assessment, pilot qualifications, and airspace coordination.",
            Priority = "Mandatory",
            ReferenceSource = "EASA Regulation (EU) 2019/947, Article 12",
            Deadline = "30 days before operation"
        });

        result.Obligations.Add(new ComplianceObligation
        {
            Type = "BVLOS",
            Description = "Detect-and-Avoid (DAA) system required",
            DetailedGuidance = "Implement cooperative (ADS-B, FLARM) and/or non-cooperative (radar, optical) DAA with performance standards per OSO#18.",
            Priority = "Mandatory",
            ReferenceSource = "SORA 2.5 OSO#18"
        });

        result.Obligations.Add(new ComplianceObligation
        {
            Type = "BVLOS",
            Description = "Remote pilot BVLOS certification",
            DetailedGuidance = "Pilot must hold specific BVLOS training and certification per national CAA requirements.",
            Priority = "Mandatory",
            ReferenceSource = "EASA Part-UAS"
        });
    }

    private void AddSpecialUseAreaObligations(ComplianceRequest request, ComplianceResult result)
    {
        if (request.SpecialUseAreas?.Contains("Airport") == true || request.SpecialUseAreas?.Contains("Heliport") == true)
        {
            result.Obligations.Add(new ComplianceObligation
            {
                Type = "Airspace Auth",
                Description = "Airport/Heliport proximity authorization",
                DetailedGuidance = "Operations near airports/heliports (within CTR/ATZ) require explicit ATC coordination and may require transponder.",
                Priority = "Mandatory",
                ReferenceSource = "ICAO Annex 2, EASA Regulation (EU) 2019/947"
            });
        }

        if (request.SpecialUseAreas?.Contains("Military") == true)
        {
            result.Obligations.Add(new ComplianceObligation
            {
                Type = "Airspace Auth",
                Description = "Military coordination required",
                DetailedGuidance = "Contact military authority for clearance. Operation may be prohibited or restricted to specific time windows.",
                Priority = "Mandatory",
                ReferenceSource = "National regulations"
            });
        }
    }

    private string DetermineComplianceStatus(ComplianceResult result)
    {
        var mandatoryCount = result.Obligations.Count(o => o.Priority == "Mandatory");
        if (mandatoryCount == 0)
        {
            return "Ready";
        }
        if (mandatoryCount > 0)
        {
            return "Pending"; // Operator must fulfill obligations
        }
        return "Unknown";
    }

    private List<string> GetRequiredOSOs(string? arc)
    {
        // Simplified OSO mapping (full matrix in SORA 2.5 Annex E)
        return arc switch
        {
            "a" => new List<string> { "OSO#1: Low", "OSO#2: Low", "OSO#3: Low" },
            "b" => new List<string> { "OSO#1: Medium", "OSO#2: Medium", "OSO#3: Medium", "OSO#4: Low" },
            "c" => new List<string> { "OSO#1: High", "OSO#2: High", "OSO#3: High", "OSO#4: Medium", "OSO#18: Medium" },
            "d" => new List<string> { "OSO#1: High", "OSO#2: High", "OSO#3: High", "OSO#4: High", "OSO#18: High", "OSO#24: High" },
            _ => new List<string>()
        };
    }

    private int? GetRequiredTMPR(string? arc)
    {
        return arc switch
        {
            "a" => 1,
            "b" => 1,
            "c" => 2, // or 3 depending on operation
            "d" => 3, // or 4
            _ => null
        };
    }

    private List<ComplianceChecklistItem> GetDocumentationChecklist(ComplianceRequest request)
    {
        var checklist = new List<ComplianceChecklistItem>
        {
            new ComplianceChecklistItem
            {
                Category = "Documentation",
                Item = "Operations Manual (OpManual)",
                IsMandatory = request.ARC == "c" || request.ARC == "d",
                ReferenceSource = "SORA 2.5 OSO#2"
            },
            new ComplianceChecklistItem
            {
                Category = "Documentation",
                Item = "Risk Assessment (SORA)",
                IsMandatory = request.ARC == "c" || request.ARC == "d",
                ReferenceSource = "EASA Regulation (EU) 2019/947"
            },
            new ComplianceChecklistItem
            {
                Category = "Documentation",
                Item = "Pilot qualifications and training records",
                IsMandatory = true,
                ReferenceSource = "EASA Part-UAS"
            },
            new ComplianceChecklistItem
            {
                Category = "Documentation",
                Item = "UAS registration certificate",
                IsMandatory = true,
                ReferenceSource = "EASA Regulation (EU) 2019/947, Article 14"
            }
        };

        if (request.OperationType?.Contains("BVLOS", StringComparison.OrdinalIgnoreCase) == true)
        {
            checklist.Add(new ComplianceChecklistItem
            {
                Category = "Documentation",
                Item = "BVLOS authorization from CAA",
                IsMandatory = true,
                ReferenceSource = "EASA Regulation (EU) 2019/947, Article 12"
            });
        }

        return checklist;
    }

    private List<ComplianceChecklistItem> GetEquipmentChecklist(ComplianceRequest request)
    {
        var checklist = new List<ComplianceChecklistItem>
        {
            new ComplianceChecklistItem
            {
                Category = "Equipment",
                Item = "Remote ID / e-Identification",
                IsMandatory = request.ARC == "c" || request.ARC == "d",
                ReferenceSource = "EASA AMC/GM to Regulation (EU) 2019/945"
            },
            new ComplianceChecklistItem
            {
                Category = "Equipment",
                Item = "Geo-fencing system",
                IsMandatory = true,
                ReferenceSource = "SORA 2.5 OSO#3"
            },
            new ComplianceChecklistItem
            {
                Category = "Equipment",
                Item = "Return-to-Home (RTH) function",
                IsMandatory = true,
                ReferenceSource = "SORA 2.5 OSO#12"
            },
            new ComplianceChecklistItem
            {
                Category = "Equipment",
                Item = "Parachute / Flight Termination System",
                IsMandatory = request.ARC == "c" || request.ARC == "d",
                Notes = "Required for higher ARC levels or high-risk areas",
                ReferenceSource = "SORA 2.5 M2"
            }
        };

        if (request.OperationType?.Contains("BVLOS", StringComparison.OrdinalIgnoreCase) == true)
        {
            checklist.Add(new ComplianceChecklistItem
            {
                Category = "Equipment",
                Item = "Detect-and-Avoid (DAA) system",
                IsMandatory = true,
                ReferenceSource = "SORA 2.5 OSO#18"
            });
        }

        return checklist;
    }

    private void ValidateReadiness(OperatorReadiness readiness)
    {
        var missingDocs = readiness.DocumentationChecklist.Where(c => c.IsMandatory).ToList();
        var missingEquip = readiness.EquipmentChecklist.Where(c => c.IsMandatory).ToList();

        if (missingDocs.Any())
        {
            readiness.ValidationNotes.Add($"{missingDocs.Count} mandatory documentation items pending");
        }

        if (missingEquip.Any())
        {
            readiness.ValidationNotes.Add($"{missingEquip.Count} mandatory equipment items pending");
        }

        if (!missingDocs.Any() && !missingEquip.Any())
        {
            readiness.AuthorizationStatus = "Ready for Submission";
            readiness.ValidationNotes.Add("All mandatory items completed. Ready to submit to CAA.");
        }
        else
        {
            readiness.AuthorizationStatus = "Incomplete";
        }
    }

    #endregion
}
